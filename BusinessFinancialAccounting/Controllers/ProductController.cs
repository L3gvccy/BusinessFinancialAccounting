using BusinessFinancialAccounting.Models;
using BusinessFinancialAccounting.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; 
using OpenTelemetry.Trace; 


namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для керування товарами користувача.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly ActivitySource ActivitySource = new ActivitySource("BusinessFinancialAccounting.ProductController");

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ВІДКРИТИЙ ендпоінт спеціально для load-тесту (JMeter).
        /// Повертає частину списку товарів без авторизації.
        /// </summary>
        [HttpGet("load-test")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> LoadTest(CancellationToken ct)
        {
            using var activity = ActivitySource.StartActivity("LoadTest SQL Query", ActivityKind.Internal);

            activity?.SetTag("component", "load-test");
            activity?.SetTag("db.system", "mssql");
            activity?.SetTag("db.operation", "SELECT");
            activity?.SetTag("loadtest.rows", 100);

            var stopwatch = Stopwatch.StartNew();

            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Take(100)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Units = p.Units,
                    Quantity = p.Quantity,
                    Price = p.Price
                })
                .ToListAsync(ct);

            stopwatch.Stop();

            activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("db.result_count", products.Count);

            return Ok(products);
        }

        /// <summary>
        /// Показує список товарів користувача.
        /// </summary>
        /// <returns>Список товарів користувача.</returns>
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Products(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Користувач не авторизований" });

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.User.Id == userId)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Units = p.Units,
                    Quantity = p.Quantity,
                    Price = p.Price
                })
                .ToListAsync(ct);

            return Ok(products);
        }

        /// <summary>
        /// Повертає сторінку редагування товару за його ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <returns>Інформація про товар або NotFound, якщо товар не існує.</returns>
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> EditProduct(int id, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && p.User.Id == userId)
                .Select(x => new ProductDTO
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Units = x.Units,
                    Quantity = x.Quantity,
                    Price = x.Price
                })
                .FirstOrDefaultAsync(ct);

            if (product == null)
                return NotFound(new { error = "Товар не знайдено" });

            return Ok(product);
        }

        /// <summary>
        /// Обробляє редагування товару та оновлює дані у базі.
        /// </summary>
        /// <param name="model">Модель з оновленими даними.</param>
        /// <returns>Повідомлення про результат оновлення.</returns>
        [HttpPost("edit")]
        public async Task<IActionResult> EditProduct([FromBody] ProductUpdateDTO model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.User.Id == userId, ct);

            if (product == null)
                return NotFound(new { error = "Товар не знайдено" });

            product.Code = model.Code;
            product.Name = model.Name!;
            product.Units = model.Units!;
            product.Quantity = model.Quantity;
            product.Price = model.Price;

            await _context.SaveChangesAsync(ct);
            return Ok(new { message = $"Товар \"{product.Name}\" було успішно оновлено!" });
        }

        /// <summary>
        /// Обробляє додавання нового товару для користувача.
        /// </summary>
        /// <param name="model">Модель з даними нового товару.</param>
        /// <returns>Повідомлення про успішне додавання або оновлення кількості.</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] ProductCreateDTO model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return Unauthorized();

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Code == model.Code && p.User.Id == userId, ct);

            if (existingProduct != null)
            {
                existingProduct.Quantity += model.Quantity;
                await _context.SaveChangesAsync(ct);

                return Ok(new { message = $"Кількість товару \"{existingProduct.Name}\" оновлено до {existingProduct.Quantity}" });
            }

            var product = new Product
            {
                Code = model.Code,
                Name = model.Name!,
                Units = model.Units!,
                Quantity = model.Quantity,
                Price = model.Price,
                User = user
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);

            return Ok(new { message = $"Товар \"{product.Name}\" було успішно додано!" });
        }

        /// <summary>
        /// Шукає товар за його кодом і повертає його дані.
        /// </summary>
        /// <param name="code">Код товару для пошуку.</param>
        /// <returns>JSON-обʼєкт з інформацією про товар або 404, якщо не знайдено.</returns>
        [HttpGet("find-by-code")]
        public async Task<IActionResult> FindByCode([FromQuery] int code, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code && p.User.Id == userId, ct);

            if (product == null)
                return NotFound(new { error = "Товар не знайдено" });

            return Ok(new ProductBriefDTO
            {
                Code = product.Code,
                Name = product.Name,
                Units = product.Units,
                Price = product.Price
            });
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var s = HttpContext.Session.GetString("UserId");
            return int.TryParse(s, out userId);
        }
    }
}
