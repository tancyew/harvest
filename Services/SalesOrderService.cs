using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class SalesOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly InventoryService _inventoryService;

    public SalesOrderService(ApplicationDbContext context, InventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<List<SalesOrder>> GetAllOrdersAsync()
    {
        return await _context.SalesOrders
            .Include(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<SalesOrder>> GetOrdersByStatusAsync(string status)
    {
        return await _context.SalesOrders
            .Include(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<SalesOrder?> GetOrderByIdAsync(int orderId)
    {
        return await _context.SalesOrders
            .Include(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);
    }

    public async Task<SalesOrder?> GetOrderByNumberAsync(string orderNumber)
    {
        return await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<List<SalesOrder>> GetOrdersByCustomerAsync(int customerId)
    {
        return await _context.SalesOrders
            .Include(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<SalesOrder> CreateOrderAsync(SalesOrder order)
    {
        order.OrderNumber = await GenerateOrderNumberAsync();
        order.CreatedDate = DateTime.UtcNow;

        // Calculate totals
        CalculateOrderTotals(order);

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<SalesOrder> UpdateOrderAsync(SalesOrder order)
    {
        order.ModifiedDate = DateTime.UtcNow;
        CalculateOrderTotals(order);

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        var order = await _context.SalesOrders.FindAsync(orderId);
        if (order == null)
            return false;

        order.Status = status;
        order.ModifiedDate = DateTime.UtcNow;

        // If status is Completed, reduce inventory
        if (status == "Completed")
        {
            await DeductInventoryAsync(orderId);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

        if (order == null)
            return false;

        // Only allow deletion if no invoice exists
        if (order.Invoice != null)
            return false;

        _context.SalesOrders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddLineItemAsync(int orderId, OrderLineItem lineItem)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            return false;

        lineItem.SalesOrderId = orderId;
        lineItem.LineTotal = lineItem.Quantity * lineItem.UnitPrice - lineItem.DiscountAmount;

        _context.OrderLineItems.Add(lineItem);

        // Recalculate order totals
        order.LineItems.Add(lineItem);
        CalculateOrderTotals(order);
        order.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveLineItemAsync(int lineItemId)
    {
        var lineItem = await _context.OrderLineItems
            .Include(l => l.SalesOrder)
            .FirstOrDefaultAsync(l => l.LineItemId == lineItemId);

        if (lineItem == null)
            return false;

        var order = lineItem.SalesOrder;
        _context.OrderLineItems.Remove(lineItem);

        // Recalculate order totals
        order.LineItems.Remove(lineItem);
        CalculateOrderTotals(order);
        order.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private void CalculateOrderTotals(SalesOrder order)
    {
        order.SubTotal = order.LineItems.Sum(l => l.LineTotal);
        order.TotalAmount = order.SubTotal - order.DiscountAmount + order.TaxAmount;
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"SO-{date:yyyyMMdd}";

        var lastOrder = await _context.SalesOrders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
            return $"{prefix}-001";

        var lastNumber = int.Parse(lastOrder.OrderNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }

    private async Task DeductInventoryAsync(int orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            return;

        foreach (var lineItem in order.LineItems)
        {
            await _inventoryService.IssueStockAsync(
                lineItem.ProductId,
                lineItem.Quantity,
                order.OrderNumber,
                $"Sales Order: {order.OrderNumber}"
            );
        }
    }

    public async Task<List<SalesOrder>> GetOrdersByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.SalesOrders
            .Include(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(o => o.LineItems)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
    }
}
