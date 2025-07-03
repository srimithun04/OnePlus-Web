
// --- Order.cs ---
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlus.Models
{
    public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled, Returned }

    public class Order
    {
        [Key]
        public int OrderId { get; set; } // Changed from Guid to int

        public int UserId { get; set; } // Changed from Guid to int
        public User User { get; set; }

        public int ShippingAddressId { get; set; }
        public ShippingAddress ShippingAddress { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
