
// --- ProductCategory.cs ---
namespace YourProject.Models
{
    public class ProductCategory
    {
        public int ProductId { get; set; } // Changed from Guid to int
        public Product Product { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
