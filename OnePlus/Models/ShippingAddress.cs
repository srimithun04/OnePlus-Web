
// --- ShippingAddress.cs ---
using System;
using System.ComponentModel.DataAnnotations;

namespace YourProject.Models
{
    public class ShippingAddress
    {
        [Key]
        public int AddressId { get; set; }

        public int UserId { get; set; } // Changed from Guid to int
        public User User { get; set; }

        [Required]
        [StringLength(255)]
        public string AddressLine1 { get; set; }

        [StringLength(255)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
