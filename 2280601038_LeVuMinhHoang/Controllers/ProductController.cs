using _2280601038_LeVuMinhHoang.Repository;
using Microsoft.AspNetCore.Mvc;
using _2280601038_LeVuMinhHoang.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace _2280601038_LeVuMinhHoang.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository,
                               ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["SearchMessage"] = "Vui lòng nhập từ khóa tìm kiếm";
                return RedirectToAction(nameof(Index));
            }

            var products = await _productRepository.SearchAsync(searchTerm);

            if (products == null || !products.Any())
            {
                TempData["SearchMessage"] = $"Không tìm thấy sản phẩm nào với từ khóa '{searchTerm}'";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SearchTerm = searchTerm;
            return View("Index", products);
        }
    }
}