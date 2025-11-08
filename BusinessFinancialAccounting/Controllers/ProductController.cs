using System.ComponentModel.DataAnnotations;
using BusinessFinancialAccounting.Models.DTO;
using BusinessFinancialAccounting.Controllers.DTO;
using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Показує список товарів користувача.
        /// </summary>
        /// <returns>Представлення списку товарів користувача.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.User.Id == userId)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Units = p.Units,
                    Quantity = p.Quantity, // decimal
                    Price = p.Price
                })
                .ToListAsync(ct);

            return Ok(products);
        }

        /// <summary>
        /// Повертає сторінку редагування товару за його ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <returns>Представлення для редагування або NotFound, якщо товар не існує.</returns>
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Обробляє редагування товару та оновлює дані у базі.
        /// </summary>
        /// <param name="product">Модель з оновленими даними.</param>
        /// <returns>
        /// Повертає View з помилкою, якщо дані некоректні, або Redirect на список товарів після успішного оновлення.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (product.Code <= 0 || product.Name == null || product.Quantity <= 0 || product.Price <= 0)
            {
                TempData["AlertMsg"] = $"Заповніть всі поля для оновлення товару";
                TempData["AlertType"] = "danger";
                return View(product);
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            TempData["AlertMsg"] = $"Товар \"{product.Name}\" було успішно оновлено!";
            TempData["AlertType"] = "success";
            return RedirectToAction("Products");
        }

        /// <summary>
        /// Повертає сторінку для додавання нового товару.
        /// </summary>
        /// <returns>Представлення для додавання товару.</returns>
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        /// <summary>
        /// Обробляє додавання нового товару для користувача.
        /// </summary>
        /// <param name="product">Модель з даними нового товару.</param>
        /// <returns>
        /// Повертає View з помилкою, якщо дані некоректні, або Redirect на список товарів після успішного додавання/оновлення.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (product.Code <= 0 || product.Name == null || product.Quantity <= 0 || product.Price <= 0)
            {
                TempData["AlertMsg"] = $"Заповніть всі поля для додавання товару";
                TempData["AlertType"] = "danger";
                return View(product);
            }
                

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Code == product.Code && p.User.Id == userId);

            if (existingProduct != null)
            {
                existingProduct.Quantity += product.Quantity;
                _context.Products.Update(existingProduct);
                TempData["AlertMsg"] = $"Кількість товару \"{product.Name}\" було оновлено до {existingProduct.Quantity}!";
                TempData["AlertType"] = "success";
            }
            else
            {
                product.User = user;
                _context.Products.Add(product);
                TempData["AlertMsg"] = $"Товар \"{product.Name}\" було успішно додано!";
                TempData["AlertType"] = "success";
            };

            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        /// <summary>
        /// Шукає товар за його кодом і повертає JSON-обʼєкт з інформацією.
        /// </summary>
        /// <param name="code">Код товару для пошуку.</param>
        /// <returns>
        /// JSON-обʼєкт з полями Code, Name, Units, Price,
        /// або null, якщо товар не знайдено.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> FindByCode(int code)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == code);

            if (product == null)
                return Json(null);

            return Json(new
            {
                product.Code,
                product.Name,
                product.Units,
                product.Price
            });
        }
    }
}
