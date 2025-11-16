using BusinessFinancialAccounting.Models;
using BusinessFinancialAccounting.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для управління продажами та роботою з кошиком.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : Controller
    {
        private readonly AppDbContext _context;
        private const string CART_KEY = "POS_CART_QTY"; // { productId : quantity }

        public SaleController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Спроба отримати ID користувача з сесії.
        /// </summary>
        /// <returns>ID користувача, або null, якщо користувач не авторизований.</returns>
        private int? TryGetUserId()
        {
            var s = HttpContext.Session.GetString("UserId");
            return string.IsNullOrEmpty(s) ? (int?)null : int.Parse(s);
        }

        /// <summary>
        /// Додавання товару до кошику за кодом.
        /// </summary>
        /// <param name="model">DTO для додавання товару за кодом</param>
        /// <returns>
        /// Повертає інформацію про доданий товар, повідомлення та максимальну кількість, або помилку, якщо товар не знайдено чи недостатньо залишку.
        /// </returns>
        [HttpPost("add-by-code")]
        public async Task<IActionResult> AddByCode([FromBody] AddByCodeDTO model)
        {
            var userId = TryGetUserId();
            if (userId == null) return Unauthorized();
            string message = "";

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == userId && p.Code == model.Code);

            if (product == null)
            {
                message = $"Товар з кодом {model.Code} не знайдено";
                return NotFound(new { message = message });
            }


            if (model.Quantity + 1 > product.Quantity)
            {
                message = $"Недостатньо залишку для \"{product.Name}\": доступно {product.Quantity} {product.Units}.";
                return BadRequest(new { message = message });
            }

            var productToAdd = new ProductToAddDTO { Code = product.Code, Name = product.Name, Price = product.Price, Units = product.Units };
            var maxQty = product.Quantity;
            message = $"Товар \"{product.Name}\" додано до кошику";
            return Ok(new { product = productToAdd, message = message, maxQty = maxQty });
        }

        /// <summary>
        /// Обробка зміни кількості товару
        /// </summary>
        /// <param name="model">Модель для зміни кількості товару в кошику</param>
        /// <returns>
        /// Максимальну кількість товару, або помилку, якщо недостатньо залишку.
        /// </returns>
        [HttpPost("change-qty")]
        public async Task<IActionResult> ChangeQty([FromBody] ChangeQuantityDTO model)
        {
            var userId = TryGetUserId();
            if (userId == null) return Unauthorized();

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == userId && p.Code == model.Code);

            if (model.Quantity > product.Quantity)
            {
                var maxQuantity = product.Quantity;
                var message = $"Недостатньо залишку для \"{product.Name}\": максимум {product.Quantity} {product.Units}.";
                return BadRequest(new { maxQuantity = maxQuantity, message = message });
            }

            return Ok();
        }

        /// <summary>
        /// Виконує оплату товарів у кошику.
        /// </summary>
        /// <param name="model">DTO для передачі товарів в кошишку та методу оплати</param>
        /// <returns>
        /// Збережену квитанцію з інформацією про оплату, або помилку, якщо виникла проблема під час оплати.
        /// </returns>
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PayRequestDTO model)
        {
            var userId = TryGetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Потрібно увійти в систему" });

            var method = (model?.Method ?? "").ToLowerInvariant();
            if (method != "cash" && method != "card")
                return BadRequest(new { message = "Невідомий метод оплати" });

            if (model.Products == null || !model.Products.Any())
                return BadRequest(new { message = "Кошик порожній" });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return Unauthorized(new { message = "Користувача не знайдено" });

            decimal total = 0;
            var validProducts = new List<Product>();

            foreach (var item in model.Products)
            {
                var product = await _context.Products
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.User.Id == userId && p.Code == item.Code);

                if (product == null)
                    return NotFound(new { message = $"Товар з кодом {item.Code} не знайдено" });

                if (item.Quantity > product.Quantity)
                    return BadRequest(new
                    {
                        message = $"Недостатньо залишку для \"{product.Name}\": максимум {product.Quantity} {product.Units}.",
                        maxQty = product.Quantity
                    });

                validProducts.Add(product);
                total += product.Price * item.Quantity;
            }

            var receipt = new Receipt
            {
                User = user,
                TotalPrice = total,
                TimeStamp = DateTime.Now,
                PaymentMethod = method
            };
            _context.Reciepts.Add(receipt);

            foreach (var item in model.Products)
            {
                var product = validProducts.First(p => p.Code == item.Code);

                _context.ReceiptProducts.Add(new ReceiptProduct
                {
                    Code = product.Code,
                    Name = product.Name,
                    Units = product.Units,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = Math.Round(product.Price * item.Quantity, 2),
                    Receipt = receipt
                });

                product.Quantity -= item.Quantity;
            }

            var register = await _context.CashRegisters
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.User.Id == userId);

            if (register == null)
            {
                register = new CashRegister { User = user };
                _context.CashRegisters.Add(register);
            }

            var inc = (int)Math.Round(total, MidpointRounding.AwayFromZero);
            string paymentInfo;

            if (method == "cash")
            {
                register.CashBalance += inc;
                paymentInfo = $"Оплата ГОТІВКОЮ: {inc} грн.";
            }
            else
            {
                register.CardBalance += inc;
                paymentInfo = $"Оплата КАРТКОЮ: {inc} грн.";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = paymentInfo,
                total,
                method,
                timestamp = receipt.TimeStamp
            });
        }

    }
}
