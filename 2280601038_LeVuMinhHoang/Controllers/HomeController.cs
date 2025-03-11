using _2280601038_LeVuMinhHoang.Repository;
using Microsoft.AspNetCore.Mvc;
using _2280601038_LeVuMinhHoang.Models;

namespace _2280601038_LeVuMinhHoang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
    }
}