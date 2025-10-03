using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessFinancialAccounting.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string fullName, string password, string confirmPassword, string phone, string email, int cashBalance, int cardBalance)
        {

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають!");
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Такий логін вже існує!");
                return View();
            }

            var user = new User
            {
                Username = username,
                FullName = fullName,
                Password = password,
                Phone = phone,
                Email = email
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var cashRegister = new CashRegister
            {
                User = user,
                CashBalance = cashBalance,
                CardBalance = cardBalance
            };

            _context.CashRegisters.Add(cashRegister);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Невірний логін або пароль");
                return View();
            }

            return RedirectToAction("Index", "Home"); 
        }
    }
}
