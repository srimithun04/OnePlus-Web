

// --- Payment.cs ---
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    public enum PaymentStatus { Pending, Completed, Failed, Refunded }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; } // Changed from Guid to int

        public int OrderId { get; set; } // Changed from Guid to int
        public Order Order { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [StringLength(255)]
        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}
