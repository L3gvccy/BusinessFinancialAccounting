using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessFinancialAccounting.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Products()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var products = await _context.Products.Include(p => p.User).Where(p => p.User.Id == userId).ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

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

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
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
