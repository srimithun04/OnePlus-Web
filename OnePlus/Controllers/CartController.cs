// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using OnePlus.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http; // For HttpContextAccessor
using System; // For Exception and ArgumentException

namespace OnePlus.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to securely get the current logged-in user's ID
        private int? GetCurrentUserId()
        {
            // THIS IS CRUCIAL: Ensure your UAM (User Access Management) login
            // sets the "UserId" in the session after a successful login.
            // Example: HttpContext.Session.SetString("UserId", user.UserId.ToString());
            var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");

            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null; // Return null if user ID is not found or not parsable
        }

        // GET: Cart/Index (Displays the cart page)
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to view your cart.";
                return RedirectToAction("Login", "Uam"); // Redirect to UAM Login if not logged in
            }

            var cartItems = await _cartService.GetCartItemsAsync(userId.Value);
            decimal cartTotal = await _cartService.GetCartTotalAsync(userId.Value);

            ViewData["CartTotal"] = cartTotal;
            // The view name "Index" matches Cart/Index.cshtml
            return View("Index", cartItems);
        }

        // POST: Cart/AddToCart (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid product or quantity." });
            }

            try
            {
                await _cartService.AddToCartAsync(userId.Value, request.ProductId, request.Quantity);
                int newItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                return Json(new { success = true, message = "Product added to cart!", newItemCount = newItemCount });
            }
            catch (ArgumentException ex)
            {
                // Catches product not found/inactive or invalid ID exceptions from service
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging in production
                Console.WriteLine($"Error adding to cart for User {userId.Value}, Product {request.ProductId}: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred while adding to cart." });
            }
        }

        // POST: Cart/RemoveFromCart (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody] CartItemRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            if (request == null || request.CartItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid cart item ID." });
            }

            try
            {
                await _cartService.RemoveFromCartAsync(request.CartItemId, userId.Value);
                decimal newTotal = await _cartService.GetCartTotalAsync(userId.Value);
                int newItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                return Json(new { success = true, message = "Product removed from cart.", newTotal = newTotal.ToString("C"), newItemCount = newItemCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing cart item {request.CartItemId} for User {userId.Value}: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while removing the item." });
            }
        }

        // POST: Cart/UpdateQuantity (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            if (request == null || request.CartItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid cart item ID." });
            }
            // Allow quantity to be 0 or less, as the service will handle removal
            // if (request.Quantity < 0) return Json(new { success = false, message = "Quantity cannot be negative." });


            try
            {
                await _cartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity, userId.Value);

                decimal newTotal = await _cartService.GetCartTotalAsync(userId.Value);
                int newItemCount = await _cartService.GetCartItemCountAsync(userId.Value);

                string message = "Quantity updated.";
                if (request.Quantity <= 0)
                {
                    message = "Item removed due to zero quantity.";
                }

                return Json(new { success = true, message = message, newTotal = newTotal.ToString("C"), newItemCount = newItemCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quantity for cart item {request.CartItemId} for User {userId.Value}: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating the quantity." });
            }
        }

        // POST: Cart/ClearCart (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            try
            {
                await _cartService.ClearCartAsync(userId.Value);
                return Json(new { success = true, message = "Cart cleared.", newTotal = 0.ToString("C"), newItemCount = 0 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cart for User {userId.Value}: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while clearing the cart." });
            }
        }

        // GET: Cart/GetCartItemCount (AJAX endpoint for header/navbar)
        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = true, count = 0 });
            }

            try
            {
                var count = await _cartService.GetCartItemCountAsync(userId.Value);
                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart item count for User {userId.Value}: {ex.Message}");
                return Json(new { success = false, count = 0, message = "Error fetching cart count." });
            }
        }
    }

    // --- Request Models for JSON Deserialization ---
    // These help with clarity and strong-typing for [FromBody] parameters
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemRequest
    {
        public int CartItemId { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}