using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnePlus.Data;
using OnePlus.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductService(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Gets all products and filters them based on a search term.
        /// The search checks the Product ID, Name, and associated Category names.
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllProductsAsync(string searchTerm = null)
        {
            // Start with the base query including related categories for searching
            var query = _context.Products
                                .Include(p => p.ProductCategories)
                                .ThenInclude(pc => pc.Category)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Try to parse the search term as an integer for ID search
                int.TryParse(searchTerm, out int searchId);

                query = query.Where(p =>
                    // Search by Product Name (contains, case-insensitive)
                    EF.Functions.Like(p.Name, $"%{searchTerm}%") ||

                    // Search by Product ID (exact match)
                    p.ProductId == searchId ||

                    // Search by Sku (contains, case-insensitive)
                    EF.Functions.Like(p.Sku, $"%{searchTerm}%") ||

                    // Search by Category Name (any category name contains the search term, case-insensitive)
                    p.ProductCategories.Any(pc => EF.Functions.Like(pc.Category.Name, $"%{searchTerm}%"))
                );
            }

            return await query.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> AddCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || await _context.Categories.AnyAsync(c => EF.Functions.Like(c.Name, categoryName)))
            {
                return null;
            }

            var category = new Category { Name = categoryName };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int categoryId)
        {
            // Check if any product is using this category
            var isCategoryInUse = await _context.ProductCategories.AnyAsync(pc => pc.CategoryId == categoryId);

            if (isCategoryInUse)
            {
                return (false, "This category cannot be deleted because it is currently assigned to one or more products.");
            }

            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                return (false, "Category not found.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return (true, "Category deleted successfully.");
        }

        public async Task AddProductAsync(Product product, IFormFile imageFile, int[] selectedCategoryIds)
        {
            if (imageFile != null)
            {
                product.ImageUrl = await SaveImageAsync(imageFile);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (selectedCategoryIds != null)
            {
                foreach (var catId in selectedCategoryIds)
                {
                    _context.ProductCategories.Add(new ProductCategory { ProductId = product.ProductId, CategoryId = catId });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateProductAsync(Product product, IFormFile imageFile, int[] selectedCategoryIds)
        {
            var existingProduct = await _context.Products
                                                .Include(p => p.ProductCategories)
                                                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

            if (existingProduct == null) return;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }
                existingProduct.ImageUrl = await SaveImageAsync(imageFile);
            }

            _context.Entry(existingProduct).CurrentValues.SetValues(product);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Remove old categories
            existingProduct.ProductCategories.Clear();

            // Add new categories
            if (selectedCategoryIds != null)
            {
                foreach (var catId in selectedCategoryIds)
                {
                    existingProduct.ProductCategories.Add(new ProductCategory { CategoryId = catId });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string productImagesPath = Path.Combine(wwwRootPath, "images", "products");

            if (!Directory.Exists(productImagesPath))
            {
                Directory.CreateDirectory(productImagesPath);
            }

            string finalPath = Path.Combine(productImagesPath, fileName);

            using (var fileStream = new FileStream(finalPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/products/" + fileName;
        }
    }
}