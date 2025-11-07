using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class CustomerPricingService
{
    private readonly ApplicationDbContext _context;

    public CustomerPricingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerPricing>> GetAllPricingsAsync()
    {
        return await _context.CustomerPricings
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Include(c => c.Product)
            .OrderBy(c => c.Customer.BusinessName)
            .ThenBy(c => c.Product.Name)
            .ToListAsync();
    }

    public async Task<List<CustomerPricing>> GetPricingsByCustomerAsync(int customerId)
    {
        return await _context.CustomerPricings
            .Include(c => c.Product)
            .Where(c => c.CustomerId == customerId && c.IsActive)
            .OrderBy(c => c.Product.Name)
            .ToListAsync();
    }

    public async Task<List<CustomerPricing>> GetPricingsByProductAsync(int productId)
    {
        return await _context.CustomerPricings
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Where(c => c.ProductId == productId && c.IsActive)
            .OrderBy(c => c.Customer.BusinessName)
            .ToListAsync();
    }

    public async Task<CustomerPricing?> GetPricingByIdAsync(int pricingId)
    {
        return await _context.CustomerPricings
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.CustomerPricingId == pricingId);
    }

    public async Task<CustomerPricing?> GetActivePricingAsync(int customerId, int productId)
    {
        var now = DateTime.UtcNow;

        return await _context.CustomerPricings
            .Where(c => c.CustomerId == customerId &&
                       c.ProductId == productId &&
                       c.IsActive &&
                       (!c.EffectiveFrom.HasValue || c.EffectiveFrom <= now) &&
                       (!c.EffectiveTo.HasValue || c.EffectiveTo >= now))
            .OrderByDescending(c => c.Priority)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> GetCustomerPriceAsync(int customerId, int productId, decimal basePrice)
    {
        var pricing = await GetActivePricingAsync(customerId, productId);

        if (pricing == null)
            return basePrice;

        if (pricing.PricingType == "DirectPricing" && pricing.DirectPrice.HasValue)
            return pricing.DirectPrice.Value;

        if (pricing.PricingType == "PercentageDiscount" && pricing.DiscountPercentage.HasValue)
            return basePrice * (1 - pricing.DiscountPercentage.Value / 100);

        return basePrice;
    }

    public async Task<CustomerPricing> CreatePricingAsync(CustomerPricing pricing)
    {
        pricing.CreatedDate = DateTime.UtcNow;
        _context.CustomerPricings.Add(pricing);
        await _context.SaveChangesAsync();
        return pricing;
    }

    public async Task<CustomerPricing> UpdatePricingAsync(CustomerPricing pricing)
    {
        pricing.ModifiedDate = DateTime.UtcNow;
        _context.CustomerPricings.Update(pricing);
        await _context.SaveChangesAsync();
        return pricing;
    }

    public async Task<bool> DeletePricingAsync(int pricingId)
    {
        var pricing = await _context.CustomerPricings.FindAsync(pricingId);
        if (pricing == null)
            return false;

        _context.CustomerPricings.Remove(pricing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivatePricingAsync(int pricingId)
    {
        var pricing = await _context.CustomerPricings.FindAsync(pricingId);
        if (pricing == null)
            return false;

        pricing.IsActive = false;
        pricing.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivatePricingAsync(int pricingId)
    {
        var pricing = await _context.CustomerPricings.FindAsync(pricingId);
        if (pricing == null)
            return false;

        pricing.IsActive = true;
        pricing.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CustomerPricing>> GetExpiredPricingsAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.CustomerPricings
            .Include(c => c.Customer)
            .Include(c => c.Product)
            .Where(c => c.IsActive &&
                       c.EffectiveTo.HasValue &&
                       c.EffectiveTo < now)
            .ToListAsync();
    }

    public async Task<List<CustomerPricing>> GetUpcomingPricingsAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.CustomerPricings
            .Include(c => c.Customer)
            .Include(c => c.Product)
            .Where(c => c.IsActive &&
                       c.EffectiveFrom.HasValue &&
                       c.EffectiveFrom > now)
            .OrderBy(c => c.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<bool> BulkCreatePricingAsync(int customerId, List<int> productIds, string pricingType, decimal? directPrice = null, decimal? discountPercentage = null)
    {
        var pricings = new List<CustomerPricing>();

        foreach (var productId in productIds)
        {
            pricings.Add(new CustomerPricing
            {
                CustomerId = customerId,
                ProductId = productId,
                PricingType = pricingType,
                DirectPrice = directPrice,
                DiscountPercentage = discountPercentage,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });
        }

        _context.CustomerPricings.AddRange(pricings);
        await _context.SaveChangesAsync();
        return true;
    }
}
