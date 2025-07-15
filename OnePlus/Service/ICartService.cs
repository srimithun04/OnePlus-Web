// Services/ICartService.cs
using OnePlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public interface ICartService
    {
        // Adds a product to the cart for a specific user.
        Task AddToCartAsync(int userId, int productId, int quantity);

        // Retrieves all cart items for a specific user, including product details.
        Task<IEnumerable<Cart>> GetCartItemsAsync(int userId);

        // Removes a specific cart item for a given user.
        Task RemoveFromCartAsync(int cartItemId, int userId);

        // Clears all items from a user's cart.
        Task ClearCartAsync(int userId);

        // Updates the quantity of a specific cart item for a user.
        // If quantity is 0 or less, the item should be removed.
        Task UpdateCartItemQuantityAsync(int cartItemId, int quantity, int userId);

        // Calculates the total price of all items in a user's cart.
        Task<decimal> GetCartTotalAsync(int userId);

        // Gets the total number of items (sum of quantities) in a user's cart.
        Task<int> GetCartItemCountAsync(int userId);
    }
}