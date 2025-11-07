using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly InvoiceService _invoiceService;
    private readonly CustomerService _customerService;

    public PaymentService(ApplicationDbContext context, InvoiceService invoiceService, CustomerService customerService)
    {
        _context = context;
        _invoiceService = invoiceService;
        _customerService = customerService;
    }

    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public async Task<List<Payment>> GetPaymentsByInvoiceAsync(int invoiceId)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment> RecordPaymentAsync(Payment payment)
    {
        payment.PaymentNumber = await GeneratePaymentNumberAsync();
        payment.CreatedDate = DateTime.UtcNow;

        _context.Payments.Add(payment);

        // Update invoice paid amount
        await _invoiceService.RecordPaymentAsync(payment.InvoiceId, payment.Amount);

        // Update customer outstanding balance
        var invoice = await _context.Invoices
            .Include(i => i.SalesOrder)
            .FirstOrDefaultAsync(i => i.InvoiceId == payment.InvoiceId);

        if (invoice != null)
        {
            await _customerService.UpdateOutstandingBalanceAsync(invoice.SalesOrder.CustomerId, -payment.Amount);
        }

        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> DeletePaymentAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            return false;

        // Reverse payment from invoice
        await _invoiceService.RecordPaymentAsync(payment.InvoiceId, -payment.Amount);

        // Reverse customer balance
        await _customerService.UpdateOutstandingBalanceAsync(payment.Invoice.SalesOrder.CustomerId, payment.Amount);

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Payments
            .Include(p => p.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .ThenInclude(o => o.Customer)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.PaymentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.PaymentDate <= endDate.Value);

        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByMethodAsync(string paymentMethod)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.PaymentMethod == paymentMethod)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    private async Task<string> GeneratePaymentNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"PAY-{date:yyyyMMdd}";

        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        if (lastPayment == null)
            return $"{prefix}-001";

        var lastNumber = int.Parse(lastPayment.PaymentNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }

    public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Payments.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.PaymentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.PaymentDate <= endDate.Value);

        return await query.SumAsync(p => p.Amount);
    }
}
