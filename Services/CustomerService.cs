using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class CustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .OrderBy(c => c.BusinessName)
            .ToListAsync();
    }

    public async Task<List<Customer>> GetActiveCustomersAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .Where(c => c.IsActive)
            .OrderBy(c => c.BusinessName)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Include(c => c.SalesOrders)
            .Include(c => c.CustomerPricings)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
    {
        return await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.CreatedDate = DateTime.UtcNow;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateOutstandingBalanceAsync(int customerId, decimal amount)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return false;

        customer.OutstandingBalance += amount;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Where(c => c.BusinessName.Contains(searchTerm) ||
                       c.User.Email.Contains(searchTerm) ||
                       c.User.Phone.Contains(searchTerm))
            .OrderBy(c => c.BusinessName)
            .ToListAsync();
    }
}
