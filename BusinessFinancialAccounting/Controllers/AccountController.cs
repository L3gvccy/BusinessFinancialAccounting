using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        /// <param name="model">Клас користувача</param>
        /// <param name="confirmPassword">Підтвердження паролю</param>
        /// <param name="cashBalance">Стратовий капітал готівки</param>
        /// <param name="cardBalance">Стартовий капітал карткою</param>
        /// <returns>Перехід на сторінку логіну</returns>
        [HttpPost]
        public IActionResult Register(User model, string confirmPassword, int cashBalance, int cardBalance)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Паролі не співпадають!");
                return View(model);
            }

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Такий логін вже існує!");
                return View(model);
            }

            _context.Users.Add(model);
            _context.SaveChanges();

            var cashRegister = new CashRegister
            {
                User = model,
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


        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string provider="Google", string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal?.Claims;
            if (claims == null) return RedirectToAction("Login");

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var oauthId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null || oauthId == null) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Username = email.Split('@')[0],
                    FullName = name ?? email,
                    Email = email,
                    Password = Guid.NewGuid().ToString(),
                    Phone = "+380000000000",
                    OAuthId = oauthId,
                    OAuthProvider = provider
                };
                _context.Users.Add(user);

                var cashRegister = new CashRegister
                {
                    User = user,
                    CashBalance = 0,
                    CardBalance = 0
                };

                _context.CashRegisters.Add(cashRegister);

                await _context.SaveChangesAsync();
            }
            else
            {
                if (string.IsNullOrEmpty(user.OAuthId))
                {
                    user.OAuthId = oauthId;
                    user.OAuthProvider = provider;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Profile()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileDataViewModel
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                IsGoogleLinked = !string.IsNullOrEmpty(user.OAuthId)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileDataViewModel model)
        {
            if (!ModelState.IsValid) return View("Profile", model);

            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Email = model.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["AlertMsg"] = "Дані профілю успішно оновлено!";
            TempData["AlertType"] = "success";

            return RedirectToAction("Profile");
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            user.Password = model.NewPassword;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["AlertMsg"] = "Пароль успішно змінено!";
            TempData["AlertType"] = "success";

            return RedirectToAction("Profile");
        }


        [HttpGet]
        public async Task<IActionResult> UnlinkGoogle()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.OAuthId = null;
                user.OAuthProvider = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            TempData["AlertMsg"] = "Google авторизація успішно відв'язана!";
            TempData["AlertType"] = "success";

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult LinkGoogle()
        {
            var redirectUrl = Url.Action(nameof(LinkGoogleCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> LinkGoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded) return RedirectToAction("Profile");

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var oauthId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var user = await _context.Users.FindAsync(userId);
            if (user != null && email == user.Email)
            {
                user.OAuthId = oauthId;
                user.OAuthProvider = "Google";
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            TempData["AlertMsg"] = "Google авторизація успішно прив'язана!";
            TempData["AlertType"] = "success";

            return RedirectToAction("Profile");
        }
    }
}
