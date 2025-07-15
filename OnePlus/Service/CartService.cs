// Services/CartService.cs
using Microsoft.EntityFrameworkCore;
using OnePlus.Data;
using OnePlus.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // For ArgumentException

namespace OnePlus.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            // Input validation
            if (userId <= 0) throw new ArgumentException("Invalid User ID.");
            if (productId <= 0) throw new ArgumentException("Invalid Product ID.");
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

            // 1. Check if the product exists and is active
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                throw new ArgumentException("Product not found or is not active.");
            }

            // 2. Find if the item already exists in the user's cart
            var existingCartItem = await _context.Carts
                                                 .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                // 3. If exists, update quantity
                existingCartItem.Quantity += quantity;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                // 4. If not, add new cart item
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };
                await _context.Carts.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Cart>> GetCartItemsAsync(int userId)
        {
            if (userId <= 0) return Enumerable.Empty<Cart>();

            return await _context.Carts
                                 .Include(c => c.Product) // Eager load product details
                                 .Where(c => c.UserId == userId)
                                 .OrderBy(c => c.AddedAt)
                                 .ToListAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId, int userId)
        {
            if (userId <= 0) return; // Cannot remove if user is not valid

            // Ensure the item belongs to the user before removing
            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.UserId == userId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            if (userId <= 0) return; // Cannot clear if user is not valid

            var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            if (cartItems.Any())
            {
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, int quantity, int userId)
        {
            if (userId <= 0) return; // Cannot update if user is not valid

            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.UserId == userId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    // If quantity is 0 or less, remove the item
                    _context.Carts.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    _context.Carts.Update(cartItem);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            if (userId <= 0) return 0m;

            return await _context.Carts
                                 .Include(c => c.Product)
                                 .Where(c => c.UserId == userId)
                                 .SumAsync(item => item.Quantity * item.Product.Price);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            if (userId <= 0) return 0;

            return await _context.Carts
                                 .Where(c => c.UserId == userId)
                                 .SumAsync(c => c.Quantity);
        }
    }
}