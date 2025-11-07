using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class InventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Inventory>> GetAllInventoryAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    public async Task<Inventory?> GetInventoryByProductIdAsync(int productId)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<List<Inventory>> GetLowStockItemsAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.QuantityAvailable <= i.ReorderLevel)
            .OrderBy(i => i.QuantityAvailable)
            .ToListAsync();
    }

    public async Task<bool> CheckStockAvailability(int productId, decimal quantity)
    {
        var inventory = await GetInventoryByProductIdAsync(productId);
        return inventory != null && inventory.QuantityAvailable >= quantity;
    }

    public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
    {
        inventory.LastUpdated = DateTime.UtcNow;
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
    {
        inventory.LastUpdated = DateTime.UtcNow;
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<bool> AdjustStockAsync(int productId, decimal quantity, string transactionType, string? reference = null, string? notes = null)
    {
        var inventory = await GetInventoryByProductIdAsync(productId);
        if (inventory == null)
            return false;

        // Update inventory quantity
        if (transactionType == "IN")
        {
            inventory.QuantityAvailable += quantity;
        }
        else if (transactionType == "OUT")
        {
            if (inventory.QuantityAvailable < quantity)
                return false; // Insufficient stock

            inventory.QuantityAvailable -= quantity;
        }
        else if (transactionType == "ADJUSTMENT")
        {
            inventory.QuantityAvailable = quantity; // Direct adjustment
        }

        inventory.LastUpdated = DateTime.UtcNow;

        // Create transaction record
        var transaction = new InventoryTransaction
        {
            InventoryId = inventory.InventoryId,
            ProductId = productId,
            Quantity = quantity,
            TransactionType = transactionType,
            Reference = reference,
            Notes = notes,
            TransactionDate = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReceiveStockAsync(int productId, decimal quantity, string? reference = null, string? notes = null)
    {
        return await AdjustStockAsync(productId, quantity, "IN", reference, notes);
    }

    public async Task<bool> IssueStockAsync(int productId, decimal quantity, string? reference = null, string? notes = null)
    {
        return await AdjustStockAsync(productId, quantity, "OUT", reference, notes);
    }

    public async Task<bool> AdjustStockLevelAsync(int productId, decimal newQuantity, string? notes = null)
    {
        return await AdjustStockAsync(productId, newQuantity, "ADJUSTMENT", null, notes);
    }

    public async Task<List<InventoryTransaction>> GetTransactionHistoryAsync(int productId, int limit = 50)
    {
        return await _context.InventoryTransactions
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<InventoryTransaction>> GetAllTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Inventory)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateReorderLevelAsync(int productId, decimal reorderLevel)
    {
        var inventory = await GetInventoryByProductIdAsync(productId);
        if (inventory == null)
            return false;

        inventory.ReorderLevel = reorderLevel;
        inventory.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
