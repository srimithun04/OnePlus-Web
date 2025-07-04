// --- Product.cs ---
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnePlus.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Sku { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public string? Specifications { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        // --- Start of Properties Moved from ViewModel ---
        [NotMapped]
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();

        [NotMapped]
        public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        // --- End of Properties Moved from ViewModel ---
    }
}