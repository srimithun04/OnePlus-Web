using Microsoft.AspNetCore.Mvc;
using OnePlus.Models;
using OnePlus.Services;
using System.Threading.Tasks;
using OnePlus.Auth; // Required for the AuthorizeRole attribute

namespace OnePlus.Controllers
{
    public class UamController : Controller
    {
        private readonly IUamService _uamService;

        public UamController(IUamService uamService)
        {
            _uamService = uamService;
        }

        // --- PROFILE ACTION ---
        [HttpGet]
        [AuthorizeRole(UserRole.Admin, UserRole.Client)]
        public async Task<IActionResult> Profile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login");
            }

            if (int.TryParse(userIdString, out int userId))
            {
                var user = await _uamService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    return View(user);
                }
            }

            return RedirectToAction("Login");
        }


        // --- LOGIN ACTIONS ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View("Login");
            }

            var user = await _uamService.LoginAsync(email, password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("UserName", user.FirstName);

                // --- UPDATED REDIRECTION LOGIC ---
                return user.Role == UserRole.Admin
                    ? RedirectToAction("Index", "Product") // Redirects admin to the product list
                    : RedirectToAction("Index", "Home");   // Redirects client to the home page
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Login");
        }

        // --- SIGNUP ACTIONS ---
        [HttpGet]
        public IActionResult Signup()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(string firstName, string lastName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The password and confirmation password do not match.");
                return View("Login");
            }

            var success = await _uamService.SignupAsync(firstName, lastName, email, password);

            if (success)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View("Login");
        }

        // --- LOGOUT ACTION ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
