using MemoryCaching.Data;
using MemoryCaching.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace MemoryCaching.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private const string ProductsCacheKey = "ProductsCache";
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            IMemoryCache memoryCache,
            ILogger<ProductService> logger)
        {
            _context = context;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            // Try to get from cache first
            if (_memoryCache.TryGetValue(ProductsCacheKey, out List<Product> products))
            {
                _logger.LogInformation("Retrieved products from cache at {Time}", DateTime.Now);
                return products;
            }

            // If not in cache, get from database
            products = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();

            _logger.LogInformation("Retrieved products from database at {Time}", DateTime.Now);

            // Set cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30)) // Cache will expire if not accessed for 30 minutes
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // Cache will expire after 1 hour regardless of access
                .SetPriority(CacheItemPriority.Normal)
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogInformation($"Cache entry {key} was evicted due to {reason}");
                }); // Register a callback to log when the cache entry is evicted

            // Save to cache
            _memoryCache.Set(ProductsCacheKey, products, cacheOptions);

            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _memoryCache.Remove(ProductsCacheKey); // Invalidate cache
            _logger.LogInformation("Added new product and invalidated cache at {Time}", DateTime.Now);
        }

        public void ClearCache()
        {
            _memoryCache.Remove(ProductsCacheKey);
            _logger.LogInformation("Manually cleared products cache");
        }
    }
}