using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusinessFinancialAccounting.Controllers
{
    public class SaleController : Controller
    {
        private readonly AppDbContext _context;
        private const string CART_KEY = "POS_CART_QTY"; // { productId : quantity }

        public SaleController(AppDbContext context)
        {
            _context = context;
        }

        private Dictionary<int, decimal> GetCart()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<int, decimal>();
            return JsonSerializer.Deserialize<Dictionary<int, decimal>>(json)
                   ?? new Dictionary<int, decimal>();
        }
        private void SaveCart(Dictionary<int, decimal> cart)
            => HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));

        private int? TryGetUserId()
        {
            var s = HttpContext.Session.GetString("UserId");
            return string.IsNullOrEmpty(s) ? (int?)null : int.Parse(s);
        }

        private async Task<(List<Product> products, Dictionary<int, decimal> qty, decimal total)>
            BuildCartDataAsync(int userId)
        {
            var qty = GetCart();
            var ids = qty.Keys.ToList();
            var products = await _context.Products
                .Include(p => p.User)
                .Where(p => p.User.Id == userId && ids.Contains(p.Id))
                .ToListAsync();

            decimal total = 0m;
            foreach (var p in products)
            {
                var q = qty.TryGetValue(p.Id, out var v) ? v : 0m;
                total += p.Price * q;
            }
            total = Math.Round(total, 2);
            return (products, qty, total);
        }

        public async Task<IActionResult> Sale()
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var (products, qty, total) = await BuildCartDataAsync(userId.Value);
            ViewBag.Qty = qty;
            ViewBag.Total = total;
            return View(products);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddByCode([FromForm] int code)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == userId && p.Code == code);

            if (product == null)
            {
                TempData["AlertMsg"] = $"Товар з кодом {code} не знайдено";
                TempData["AlertType"] = "danger";
                return RedirectToAction("Sale");
            }

           

            var cart = GetCart();
            cart.TryGetValue(product.Id, out var inCart);
            if (inCart + 1 > product.Quantity)
            {
                TempData["AlertMsg"] = $"Недостатньо залишку для \"{product.Name}\": доступно {product.Quantity} {product.Units}.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("Sale");
            }
            cart[product.Id] = inCart + 1;
            SaveCart(cart);

            return RedirectToAction("Sale");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQty([FromForm] int productId, [FromForm] decimal qty)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = GetCart();

            if (qty <= 0)
            {
                cart.Remove(productId);
                SaveCart(cart);
                return RedirectToAction("Sale");
            }

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == userId && p.Id == productId);

            if (product == null)
            {
                cart.Remove(productId);
                SaveCart(cart);
                TempData["AlertMsg"] = "Товар не знайдено або він не належить цьому користувачу.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("Sale");
            }

            if (qty > product.Quantity)
            {
                cart[productId] = product.Quantity;
                SaveCart(cart);
                TempData["AlertMsg"] = $"Недостатньо залишку для \"{product.Name}\": максимум {product.Quantity} {product.Units}.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("Sale");
            }

            cart[productId] = qty;
            SaveCart(cart);
            return RedirectToAction("Sale");
        }

        [HttpPost]
        public IActionResult Remove([FromForm] int productId)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = GetCart();
            cart.Remove(productId);
            SaveCart(cart);

            return RedirectToAction("Sale");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            SaveCart(new Dictionary<int, decimal>());
            return RedirectToAction("Sale");
        }

        [HttpPost]
        public async Task<IActionResult> Pay([FromQuery] string method) // cash|card
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            method = (method ?? "").ToLowerInvariant();
            if (method != "cash" && method != "card")
            {
                TempData["AlertMsg"] = "Невідомий метод оплати";
                TempData["AlertType"] = "danger";
                return RedirectToAction("Sale");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            var (products, qty, total) = await BuildCartDataAsync(userId.Value);
            if (!products.Any())
            {
                TempData["AlertMsg"] = "Кошик порожній";
                TempData["AlertType"] = "warning";
                return RedirectToAction("Sale");
            }

            var receipt = new Receipt
            {
                User = user,
                TotalPrice = total,
                TimeStamp = DateTime.Now
            };
            _context.Reciepts.Add(receipt);

            foreach (var p in products)
            {
                var q = qty[p.Id];
                _context.ReceiptProducts.Add(new ReceiptProduct
                {
                    Code = p.Code,
                    Name = p.Name,
                    Units = p.Units,
                    Price = p.Price,
                    Quantity = q,
                    TotalPrice = Math.Round(p.Price * q, 2),
                    Receipt = receipt
                });
                p.Quantity -= q;
            }

            var register = await _context.CashRegisters.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.User.Id == userId);

            if (register == null)
            {
                register = new CashRegister { User = user };
                _context.CashRegisters.Add(register);
            }

            var inc = (int)Math.Round(total, MidpointRounding.AwayFromZero);
            if (method == "cash")
            {
                register.CashBalance += inc;
                TempData["AlertMsg"] = $"Оплата ГОТІВКОЮ: {inc} грн.";
                TempData["AlertType"] = "success";
            }
            else
            {
                register.CardBalance += inc;
                TempData["AlertMsg"] = $"Оплата КАРТКОЮ: {inc} грн.";
                TempData["AlertType"] = "success";
            }

            await _context.SaveChangesAsync();

            SaveCart(new Dictionary<int, decimal>());

            return RedirectToAction("Sale");
        }
    }
}
