using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для керування обліковими записами користувачів, включаючи реєстрацію, вхід та вихід.
    /// </summary>
    public class AccountController : Controller
    {
        private AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Відображає сторінку реєстрації користувача.
        /// </summary>
        /// <returns>Сторінка реєстрації</returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Обробляє реєстрацію нового користувача.
        /// </summary>
        /// <param name="username">Логін</param>
        /// <param name="fullName">Повне Ім'я</param>
        /// <param name="password">Пароль</param>
        /// <param name="confirmPassword">Підтвердження паролю</param>
        /// <param name="phone">Номер телефону</param>
        /// <param name="email">Електронна пошта</param>
        /// <param name="cashBalance">Стратовий капітал готівки</param>
        /// <param name="cardBalance">Стартовий капітал карткою</param>
        /// <returns>Перехід на сторінку логіну</returns>
        [HttpPost]
        public IActionResult Register(string username, string fullName, string password, string confirmPassword, string phone, string email, int cashBalance, int cardBalance)
        {

            if (password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Паролі не співпадають!");
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Такий логін вже існує!");
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

        /// <summary>
        /// Відображає сторінку входу користувача.
        /// </summary>
        /// <returns>Сторінка входу користувача</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Обробляє вхід користувача.
        /// </summary>
        /// <param name="username">Логін</param>
        /// <param name="password">Пароль</param>
        /// <returns>Перехід на головну сторінку в разі успішної автентифікації</returns>
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Username", "Невірний логін або пароль");
            return View();
        }

        /// <summary>
        /// Обробляє вихід користувача з системи.
        /// </summary>
        /// <returns>Перехід на головну сторінку</returns>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
