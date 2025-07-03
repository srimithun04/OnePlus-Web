using Microsoft.AspNetCore.Mvc;
using OnePlus.Auth;
using System.Threading.Tasks;
using OnePlus.Models;
using OnePlus.Services;

namespace OnePlus.Controllers
{
    public class UamController : Controller
    {
        private readonly IUamService _uamService;

        public UamController(IUamService uamService)
        {
            _uamService = uamService;
        }

        // --- NEW PROFILE ACTION ---
        [HttpGet]
        [AuthorizeRole(UserRole.Admin, UserRole.Client)]
        public async Task<IActionResult> Profile()
        {
            // Check if the UserId is present in the session
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                // If not logged in, redirect to the login page
                return RedirectToAction("Login");
            }

            if (int.TryParse(userIdString, out int userId))
            {
                // Fetch the user from the database using the new service method
                var user = await _uamService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    // If user is found, pass the user object to the view
                    return View(user);
                }
            }

            // If user is not found or ID is invalid, redirect to login
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
                return View();
            }

            var user = await _uamService.LoginAsync(email, password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("UserName", user.FirstName);

                return user.Role == UserRole.Admin
                    ? RedirectToAction("Dashboard", "Admin")
                    : RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
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
