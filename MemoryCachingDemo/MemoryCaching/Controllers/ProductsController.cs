using MemoryCaching.Models;
using MemoryCaching.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoryCaching.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productService.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ClearCache()
        {
            _productService.ClearCache();
            return RedirectToAction(nameof(Index));
        }
    }
}
