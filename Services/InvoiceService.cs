using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class InvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetInvoicesByStatusAsync(string status)
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .Include(i => i.Payments)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .ThenInclude(c => c.User)
            .Include(i => i.SalesOrder.LineItems)
            .ThenInclude(l => l.Product)
            .Include(i => i.Payments)
            .Include(i => i.CreditNotes)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<Invoice> CreateInvoiceFromOrderAsync(int orderId, int paymentTermDays = 30)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

        if (order == null)
            throw new Exception("Order not found");

        var invoice = new Invoice
        {
            SalesOrderId = orderId,
            InvoiceNumber = await GenerateInvoiceNumberAsync(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(paymentTermDays),
            Status = "Draft",
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            PaidAmount = 0,
            BalanceAmount = order.TotalAmount,
            PaymentTerms = $"Net {paymentTermDays} days",
            CreatedDate = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> UpdateInvoiceStatusAsync(int invoiceId, string status)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
            return false;

        invoice.Status = status;

        if (status == "Sent" && !invoice.SentDate.HasValue)
            invoice.SentDate = DateTime.UtcNow;

        if (status == "Paid" && !invoice.PaidDate.HasValue)
            invoice.PaidDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecordPaymentAsync(int invoiceId, decimal amount)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
            return false;

        invoice.PaidAmount += amount;
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceAmount <= 0)
        {
            invoice.Status = "Paid";
            invoice.PaidDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApplyCreditNoteAsync(int invoiceId, decimal amount)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
            return false;

        invoice.BalanceAmount -= amount;

        if (invoice.BalanceAmount <= 0)
        {
            invoice.Status = "Paid";
            invoice.PaidDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Invoice>> GetOverdueInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .Where(i => i.Status != "Paid" && i.Status != "Cancelled" && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> GetInvoicesByCustomerAsync(int customerId)
    {
        return await _context.Invoices
            .Include(i => i.SalesOrder)
            .Include(i => i.Payments)
            .Where(i => i.SalesOrder.CustomerId == customerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"INV-{date:yyyyMMdd}";

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
            return $"{prefix}-001";

        var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }

    public async Task<decimal> GetTotalOutstandingAsync()
    {
        return await _context.Invoices
            .Where(i => i.Status != "Paid" && i.Status != "Cancelled")
            .SumAsync(i => i.BalanceAmount);
    }
}
