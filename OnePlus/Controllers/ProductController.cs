using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnePlus.Auth;
using OnePlus.Models;
using OnePlus.Services;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePlus.Controllers
{
    [AuthorizeRole(UserRole.Admin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            // Pass the search term to the service
            var products = await _productService.GetAllProductsAsync(searchTerm);

            // Pass the search term back to the view so the search box doesn't clear
            ViewData["SearchTerm"] = searchTerm;

            return View(products);
        }

        // --- Corrected Upsert GET Action ---
        public async Task<IActionResult> Upsert(int? id)
        {
            Product productModel;
            var allCategories = await _productService.GetAllCategoriesAsync();
            var categoryList = allCategories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            });

            if (id == null || id == 0)
            {
                // This is for CREATING a new product
                productModel = new Product();
            }
            else
            {
                // This is for EDITING an existing product
                productModel = await _productService.GetProductByIdAsync(id.Value);
                if (productModel == null)
                {
                    return NotFound();
                }
                // Pre-select the categories for the existing product
                productModel.SelectedCategoryIds = productModel.ProductCategories.Select(pc => pc.CategoryId).ToArray();
            }

            // Assign the category list to the model
            productModel.CategoryList = categoryList;

            return View(productModel);
        }

        // --- Corrected Upsert POST Action ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Product model)
        {
            // We manually remove model state for non-entity properties to ensure validation works correctly.
            ModelState.Remove("ImageFile");
            ModelState.Remove("CategoryList");
            ModelState.Remove("SelectedCategoryIds");
            ModelState.Remove("ProductCategories");


            if (ModelState.IsValid)
            {
                if (model.ProductId == 0)
                {
                    await _productService.AddProductAsync(model, model.ImageFile, model.SelectedCategoryIds);
                }
                else
                {
                    await _productService.UpdateProductAsync(model, model.ImageFile, model.SelectedCategoryIds);
                }
                return RedirectToAction(nameof(Index));
            }

            // If the model is not valid, we must repopulate the category list before returning to the view
            model.CategoryList = (await _productService.GetAllCategoriesAsync()).Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] Category request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Category name cannot be empty.");
            }
            var newCategory = await _productService.AddCategoryAsync(request.Name);
            if (newCategory == null)
            {
                return BadRequest("Category already exists or failed to create.");
            }
            return Json(new { id = newCategory.CategoryId, name = newCategory.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("categoryId", out var categoryIdElement) || !categoryIdElement.TryGetInt32(out var categoryId))
            {
                return BadRequest(new { success = false, message = "Invalid Category ID." });
            }

            var (success, message) = await _productService.DeleteCategoryAsync(categoryId);

            if (!success)
            {
                return BadRequest(new { success = false, message = message });
            }

            return Json(new { success = true });
        }
    }
}