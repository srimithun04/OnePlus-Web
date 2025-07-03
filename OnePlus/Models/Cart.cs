
// --- Cart.cs ---
using System;
using System.ComponentModel.DataAnnotations;

namespace YourProject.Models
{
    public class Cart
    {
        [Key]
        public int CartItemId { get; set; }

        public int UserId { get; set; } // Changed from Guid to int
        public User User { get; set; }

        public int ProductId { get; set; } // Changed from Guid to int
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}

