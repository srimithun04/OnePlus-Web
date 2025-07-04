using Microsoft.EntityFrameworkCore;
using OnePlus.Data;
using OnePlus.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;

        public HomeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProductsForHomeAsync()
        {
            return await _context.Products
                                 .Include(p => p.ProductCategories)
                                 .ThenInclude(pc => pc.Category)
                                 .Where(p => p.IsActive)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesForHomeAsync()
        {
            return await _context.Categories
                                   .Where(c => c.ProductCategories.Any(pc => pc.Product.IsActive)) // Only get categories that have active products
                                   .ToListAsync();
        }
    }
}