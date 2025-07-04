using OnePlus.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(string searchTerm = null);

        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> AddCategoryAsync(string categoryName);
        Task AddProductAsync(Product product, IFormFile imageFile, int[] selectedCategoryIds);
        Task UpdateProductAsync(Product product, IFormFile imageFile, int[] selectedCategoryIds);
        Task DeleteProductAsync(int id);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int categoryId);
    }
}
