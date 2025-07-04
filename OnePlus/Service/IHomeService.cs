using OnePlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public interface IHomeService
    {
        Task<IEnumerable<Product>> GetProductsForHomeAsync();
        Task<IEnumerable<Category>> GetCategoriesForHomeAsync();
    }
}