# Memory Caching in ASP.NET Core - Code Explanation

# Building a Product with Database and Memory Caching
Let's build Database with Entity Framework Core and ASP.NET Core Identity, then implement memory caching.

## Step 1: Set Up the Database

### 1. Install required NuGet packages:
```
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### 2. Create Product Model

In `Models/Product.cs`:
```csharp
public class Product
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
```

### Responsibilities:
- Defines the Product entity with:
  - `Id`: Primary key
  - `Name`: Required product name
  - `Price`: Formatted as currency
  - `LastUpdated`: Auto-set to current UTC time
- Includes data annotations for validation and display formatting
---

### 3. Create ApplicationDbContext

In `Data/ApplicationDbContext.cs`:
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Seed initial product data
        builder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Price = 999.99m },
            // ... 19 more products
        );
    }
}
```

### Responsibilities:
- Inherits from `IdentityDbContext` to include ASP.NET Core Identity functionality
- Defines the `Products` DbSet for database operations
- Seeds the database with 20 sample products when the model is created
- Configures the database schema and relationships
---

### 4. Configure Database in Program.cs

Add this before `var app = builder.Build();`:
```csharp
// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
---

### 5. Add Connection String to appsettings.json

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Products;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
---

### 6. Apply Migrations

In Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```
---

## Step 2: Create Product Service with Caching

In `Services/ProductService.cs`:
```csharp
public class ProductService
{
    // Dependencies injected via constructor
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private const string ProductsCacheKey = "ProductsCache";
    private readonly ILogger<ProductService> _logger;

    // Core methods:
    public async Task<List<Product>> GetProductsAsync()
    {
        // Cache check
        if (_memoryCache.TryGetValue(ProductsCacheKey, out List<Product> products))
        {
            _logger.LogInformation("Retrieved from cache");
            return products;
        }

        // Database query if not in cache
        products = await _context.Products.OrderBy(p => p.Id).ToListAsync();
        _logger.LogInformation("Retrieved from database");

        // Cache configuration
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetPriority(CacheItemPriority.Normal)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogInformation($"Cache evicted: {reason}");
            });

        _memoryCache.Set(ProductsCacheKey, products, cacheOptions);
        return products;
    }

    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        _memoryCache.Remove(ProductsCacheKey); // Cache invalidation
        _logger.LogInformation("Added product and cleared cache");
    }

    public void ClearCache()
    {
        _memoryCache.Remove(ProductsCacheKey);
        _logger.LogInformation("Manually cleared cache");
    }
}
```

### Responsibilities:
- Manages all product-related business logic
- Implements memory caching with:
  - Sliding expiration (30 minutes of inactivity)
  - Absolute expiration (1 hour maximum)
  - Cache eviction logging
- Handles database operations
- Provides cache invalidation when products are added
- Includes comprehensive logging
---

## Step 3: Register Services in Program.cs

Add these lines before `var app = builder.Build();`:
```csharp
// Add memory cache
builder.Services.AddMemoryCache();

// Add ProductService
builder.Services.AddScoped<ProductService>();
```

## Step 4: Create ProductsController

In `Controllers/ProductsController.cs`:
```csharp
public class ProductsController : Controller
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductService productService, ILogger<ProductsController> logger)
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
```

### Responsibilities:
- Handles HTTP requests for product operations
- Uses dependency injection for services
- Implements:
  - Product listing (Index)
  - Product creation (Create)
  - Cache clearing (ClearCache)
- Includes model validation
- Manages view rendering
---

## Step 5: Create Views

### 1. Index View (`Views/Products/Index.cshtml`)
```html
@model List<Product>

<h1>Product List</h1>
<p>Last loaded: @DateTime.Now</p>

<table class="table">
    <thead>
        <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Price</th>
            <th>Last Updated</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var product in Model)
        {
            <tr>
                <td>@product.Id</td>
                <td>@product.Name</td>
                <td>@product.Price.ToString("C")</td>
                <td>@product.LastUpdated.ToLocalTime()</td>
            </tr>
        }
    </tbody>
</table>

<div class="mt-4">
    <a asp-action="Create" class="btn btn-primary">Add New Product</a>
    <a asp-action="ClearCache" class="btn btn-danger">Clear Cache</a>
</div>
```

### 2. Create View (`Views/Products/Create.cshtml`)
```html
@model Product

<h1>Add New Product</h1>

<form asp-action="Create">
    <div class="form-group">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="Price" class="control-label"></label>
        <input asp-for="Price" class="form-control" />
        <span asp-validation-for="Price" class="text-danger"></span>
    </div>
    <div class="form-group mt-3">
        <input type="submit" value="Create" class="btn btn-primary" />
        <a asp-action="Index" class="btn btn-secondary">Back to List</a>
    </div>
</form>
```
---

## Step 6: Test the Application

1. Run the application (F5)
2. Navigate to `/Products`
3. Test these scenarios:
   - Initial load (should hit database)
   - Refresh (should use cache)
   - Add new product (should invalidate cache)
   - Click "Clear Cache" button
   - Wait 5 minutes to see cache expiration
