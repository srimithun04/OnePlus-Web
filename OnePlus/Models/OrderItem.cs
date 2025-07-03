
// --- OrderItem.cs ---
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; } // Changed from Guid to int
        public Order Order { get; set; }

        public int ProductId { get; set; } // Changed from Guid to int
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PriceAtPurchase { get; set; }
    }
}
