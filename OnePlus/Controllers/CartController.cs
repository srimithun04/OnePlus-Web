// Controllers/CartController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnePlus.Services;
using System;
using System.Threading.Tasks;

namespace OnePlus.Controllers
{
    // Pro Tip: Using [Route("api/[controller]")] and HTTP method attributes ([HttpGet], [HttpPost])
    // makes the API endpoints clearer and follows RESTful conventions.
    [Route("Cart")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor)
        {
            _cartService = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper to get the current user's ID from the session.
        private int? GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            return int.TryParse(userIdString, out int userId) ? userId : (int?)null;
        }

        // GET: /Cart or /Cart/Index
        // This action renders the main cart view.
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Please log in to view your cart.";
                return RedirectToAction("Login", "Uam");
            }

            var cartItems = await _cartService.GetCartItemsAsync(userId.Value);
            ViewData["CartTotal"] = await _cartService.GetCartTotalAsync(userId.Value);

            return View("Index", cartItems);
        }

        // POST: /Cart/Add
        // Handles adding a new item to the cart.
        [HttpPost("Add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not logged in." });
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid request." });

            try
            {
                await _cartService.AddToCartAsync(userId.Value, request.ProductId, request.Quantity);
                int newItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                return Ok(new { success = true, message = "Product added to cart!", newItemCount });
            }
            catch (Exception ex)
            {
                // In production, use a proper logging framework.
                Console.WriteLine($"AddToCart Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
            }
        }

        // POST: /Cart/Update
        // A single, powerful endpoint to change quantity or remove an item (by setting quantity to 0).
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not logged in." });
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid request." });

            try
            {
                await _cartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity, userId.Value);

                // Return all the data the frontend needs to update the UI in one go.
                decimal newTotal = await _cartService.GetCartTotalAsync(userId.Value);
                int newItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                string message = request.Quantity <= 0 ? "Item removed from cart." : "Quantity updated.";

                return Ok(new { success = true, message, newTotal, newItemCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateQuantity Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the cart." });
            }
        }

        // POST: /Cart/Clear
        // Handles clearing all items from the user's cart.
        [HttpPost("Clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not logged in." });

            try
            {
                await _cartService.ClearCartAsync(userId.Value);
                return Ok(new { success = true, message = "Cart cleared." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearCart Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while clearing the cart." });
            }
        }

        // GET: /Cart/Count
        // A dedicated endpoint for fetching the cart item count, used by the navbar.
        [HttpGet("Count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Ok(new { success = true, count = 0 });

            int count = await _cartService.GetCartItemCountAsync(userId.Value);
            return Ok(new { success = true, count });
        }
    }

    // --- DTOs (Data Transfer Objects) for API Requests ---
    // Pro Tip: Using DTOs with validation attributes is a best practice for API development.
    public class AddToCartRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int ProductId { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int Quantity { get; set; }
    }

    public class UpdateQuantityRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int CartItemId { get; set; }
        // Quantity can be 0 for removal
        [System.ComponentModel.DataAnnotations.Range(0, 100)]
        public int Quantity { get; set; }
    }
}
