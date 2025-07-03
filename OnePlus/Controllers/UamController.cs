using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using YourProject.Services;
using System.Threading.Tasks;

namespace YourProject.Controllers
{
    public class UamController : Controller
    {
        private readonly IUamService _uamService;

        public UamController(IUamService uamService)
        {
            _uamService = uamService;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(string firstName, string lastName, string email, string password, string confirmPassword)
        {
            // Manual validation
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The password and confirmation password do not match.");
                return View();
            }

            var success = await _uamService.SignupAsync(firstName, lastName, email, password);

            if (success)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View();
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