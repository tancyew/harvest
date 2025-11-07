using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class JournalService
{
    private readonly ApplicationDbContext _context;

    public JournalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _context.JournalEntries
            .Include(j => j.Product)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.JournalEntries
            .Include(j => j.Product)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(j => j.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(j => j.EntryDate <= endDate.Value);

        return await query
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesByTypeAsync(string entryType)
    {
        return await _context.JournalEntries
            .Include(j => j.Product)
            .Where(j => j.EntryType == entryType)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesByProductAsync(int productId)
    {
        return await _context.JournalEntries
            .Include(j => j.Product)
            .Where(j => j.ProductId == productId)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int journalEntryId)
    {
        return await _context.JournalEntries
            .Include(j => j.Product)
            .FirstOrDefaultAsync(j => j.JournalEntryId == journalEntryId);
    }

    public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry)
    {
        entry.CreatedDate = DateTime.UtcNow;
        if (entry.EntryDate == default)
            entry.EntryDate = DateTime.UtcNow;

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry)
    {
        _context.JournalEntries.Update(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteEntryAsync(int journalEntryId)
    {
        var entry = await _context.JournalEntries.FindAsync(journalEntryId);
        if (entry == null)
            return false;

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetEntryTypesAsync()
    {
        return await _context.JournalEntries
            .Select(j => j.EntryType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<int> GetEntryCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.JournalEntries.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(j => j.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(j => j.EntryDate <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
    {
        return await _context.JournalEntries
            .Include(j => j.Product)
            .Where(j => j.Title.Contains(searchTerm) ||
                       (j.Description != null && j.Description.Contains(searchTerm)) ||
                       (j.Reference != null && j.Reference.Contains(searchTerm)))
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
    }
}
