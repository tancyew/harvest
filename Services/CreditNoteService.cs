using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class CreditNoteService
{
    private readonly ApplicationDbContext _context;
    private readonly InvoiceService _invoiceService;

    public CreditNoteService(ApplicationDbContext context, InvoiceService invoiceService)
    {
        _context = context;
        _invoiceService = invoiceService;
    }

    public async Task<List<CreditNote>> GetAllCreditNotesAsync()
    {
        return await _context.CreditNotes
            .Include(c => c.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Include(c => c.LineItems)
            .ThenInclude(l => l.Product)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();
    }

    public async Task<List<CreditNote>> GetCreditNotesByStatusAsync(string status)
    {
        return await _context.CreditNotes
            .Include(c => c.Invoice)
            .Include(c => c.Customer)
            .Include(c => c.LineItems)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();
    }

    public async Task<CreditNote?> GetCreditNoteByIdAsync(int creditNoteId)
    {
        return await _context.CreditNotes
            .Include(c => c.Invoice)
            .ThenInclude(i => i.SalesOrder)
            .ThenInclude(o => o.LineItems)
            .ThenInclude(l => l.Product)
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Include(c => c.LineItems)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(c => c.CreditNoteId == creditNoteId);
    }

    public async Task<List<CreditNote>> GetCreditNotesByCustomerAsync(int customerId)
    {
        return await _context.CreditNotes
            .Include(c => c.Invoice)
            .Include(c => c.LineItems)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();
    }

    public async Task<CreditNote> CreateCreditNoteAsync(CreditNote creditNote)
    {
        creditNote.CreditNoteNumber = await GenerateCreditNoteNumberAsync();
        creditNote.CreatedDate = DateTime.UtcNow;
        creditNote.Status = "Pending";

        _context.CreditNotes.Add(creditNote);
        await _context.SaveChangesAsync();
        return creditNote;
    }

    public async Task<CreditNote> UpdateCreditNoteAsync(CreditNote creditNote)
    {
        _context.CreditNotes.Update(creditNote);
        await _context.SaveChangesAsync();
        return creditNote;
    }

    public async Task<bool> ApproveCreditNoteAsync(int creditNoteId, string approvedBy)
    {
        var creditNote = await _context.CreditNotes.FindAsync(creditNoteId);
        if (creditNote == null)
            return false;

        creditNote.Status = "Approved";
        creditNote.ApprovedDate = DateTime.UtcNow;
        creditNote.ApprovedBy = approvedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApplyCreditNoteAsync(int creditNoteId)
    {
        var creditNote = await _context.CreditNotes.FindAsync(creditNoteId);
        if (creditNote == null || creditNote.Status != "Approved")
            return false;

        // Apply credit to invoice
        await _invoiceService.ApplyCreditNoteAsync(creditNote.InvoiceId, creditNote.Amount);

        creditNote.IsApplied = true;
        creditNote.AppliedDate = DateTime.UtcNow;
        creditNote.Status = "Applied";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectCreditNoteAsync(int creditNoteId)
    {
        var creditNote = await _context.CreditNotes.FindAsync(creditNoteId);
        if (creditNote == null)
            return false;

        creditNote.Status = "Rejected";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCreditNoteAsync(int creditNoteId)
    {
        var creditNote = await _context.CreditNotes.FindAsync(creditNoteId);
        if (creditNote == null || creditNote.IsApplied)
            return false;

        _context.CreditNotes.Remove(creditNote);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddLineItemAsync(int creditNoteId, CreditNoteLineItem lineItem)
    {
        var creditNote = await GetCreditNoteByIdAsync(creditNoteId);
        if (creditNote == null)
            return false;

        lineItem.CreditNoteId = creditNoteId;
        lineItem.Amount = lineItem.Quantity * lineItem.UnitPrice;

        _context.CreditNoteLineItems.Add(lineItem);

        // Recalculate credit note total
        creditNote.LineItems.Add(lineItem);
        creditNote.Amount = creditNote.LineItems.Sum(l => l.Amount);

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateCreditNoteNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"CN-{date:yyyyMMdd}";

        var lastCreditNote = await _context.CreditNotes
            .Where(c => c.CreditNoteNumber.StartsWith(prefix))
            .OrderByDescending(c => c.CreditNoteNumber)
            .FirstOrDefaultAsync();

        if (lastCreditNote == null)
            return $"{prefix}-001";

        var lastNumber = int.Parse(lastCreditNote.CreditNoteNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }
}
