using Microsoft.AspNetCore.Mvc;
using OnePlus.Services;
using System.Threading.Tasks;

namespace OnePlus.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch products and categories separately
            var products = await _homeService.GetProductsForHomeAsync();
            var categories = await _homeService.GetCategoriesForHomeAsync();

            // Pass categories to the view using ViewData
            ViewData["Categories"] = categories;

            // Pass the list of products as the main model
            // The view will be responsible for taking the top 10
            return View(products);
        }

        // New action for the "All Products" page
        public async Task<IActionResult> All_Products()
        {
            ViewData["Title"] = "All Products";

            // Fetch all products and categories for the dedicated page
            var allProducts = await _homeService.GetProductsForHomeAsync(); // Assuming this service method gets all products
            var categories = await _homeService.GetCategoriesForHomeAsync();

            ViewData["Categories"] = categories;

            return View(allProducts);
        }


        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
}
