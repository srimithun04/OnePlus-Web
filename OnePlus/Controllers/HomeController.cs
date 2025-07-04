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
            return View(products);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
}