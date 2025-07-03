using Microsoft.EntityFrameworkCore;
using OnePlus.Models; // Make sure this using statement is correct

namespace OnePlus.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Define a DbSet for each table in your database
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configure Many-to-Many relationship for Product and Category ---
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId);


            // When a User is deleted, we don't want their Orders to cascade delete.
            // We'll handle this in application logic (e.g., prevent user deletion if they have orders).
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete

            // Similarly, for ShippingAddress to Order.
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany() // An address can be used in many orders
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete

            // Also apply this to the Cart to be safe.
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // It's usually OK to delete cart items when a user is deleted.

            // And to ShippingAddress
            modelBuilder.Entity<ShippingAddress>()
               .HasOne(sa => sa.User)
               .WithMany(u => u.ShippingAddresses)
               .HasForeignKey(sa => sa.UserId)
               .OnDelete(DeleteBehavior.Cascade); // It's also OK to delete addresses when a user is deleted.


            // A user can only have one of each product in their cart
            modelBuilder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            // User email should be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Product SKU should be unique
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            // --- Configure Enum to String Conversion ---
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status) // CORRECTED THIS LINE
                .HasConversion<string>();
        }
    }
}