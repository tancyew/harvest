using Microsoft.EntityFrameworkCore;
using Harvest.Data;
using Harvest.Models;

namespace Harvest.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .Where(p => p.Name.Contains(searchTerm) ||
                       p.Description.Contains(searchTerm) ||
                       p.SKU.Contains(searchTerm) ||
                       p.Category.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.CreatedDate = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        product.ModifiedDate = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.IsActive = false;
        product.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateProductAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.IsActive = true;
        product.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductImage> AddProductImageAsync(ProductImage image)
    {
        image.UploadedDate = DateTime.UtcNow;
        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteProductImageAsync(int imageId)
    {
        var image = await _context.ProductImages.FindAsync(imageId);
        if (image == null)
            return false;

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryImageAsync(int imageId)
    {
        var image = await _context.ProductImages.FindAsync(imageId);
        if (image == null)
            return false;

        // Remove primary flag from all images of this product
        var productImages = await _context.ProductImages
            .Where(i => i.ProductId == image.ProductId)
            .ToListAsync();

        foreach (var img in productImages)
        {
            img.IsPrimary = false;
        }

        // Set the new primary image
        image.IsPrimary = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
