using BusinessFinancialAccounting.Models;
using BusinessFinancialAccounting.Models.DTO;
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
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Обробляє реєстрацію нового користувача.
        /// </summary>
        /// <param name="model">DTO реєстрації</param>
        /// <returns>Перехід на сторінку логіну</returns>
        [HttpPost("register")]
        public IActionResult Register(RegisterDTO model)
        {

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                return BadRequest(new { usernameErr = "Такий логін вже існує!" });
            }

            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Password = model.Password,
                Phone = model.Phone,
                Email = model.Email
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var cashRegister = new CashRegister
            {
                User = user,
                CashBalance = model.CashBalance,
                CardBalance = model.CardBalance
            };

            _context.CashRegisters.Add(cashRegister);
            _context.SaveChanges();

            return Ok(new { message = "Реєстрація успішна" });
        }

        /// <summary>
        /// Обробляє вхід користувача.
        /// </summary>
        /// <param name="model">DTO логіну</param>
        /// <returns>Перехід на головну сторінку в разі успішної автентифікації</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO model)
        {
            Console.WriteLine($"Username: '{model.Username}', Password: '{model.Password}'");

            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Порожнє ім’я або пароль" });

            var user = _context.Users
                .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
                return BadRequest(new { error = "Невірний логін або пароль" });

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            return Ok(new { message = "Ви успішно увійшли до акаунту" });
        }


        /// <summary>
        /// Обробляє вихід користувача з системи.
        /// </summary>
        /// <returns>Перехід на головну сторінку</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { alertMsg = "Ви успішно вийши з акаунуту!", alertType = "success" });
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { error = "Користувач не авторизований" });

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { error = "Некоректна сесія" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "Користувача не знайдено" });

            var dto = new ProfileDTO
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                IsGoogleLinked = !string.IsNullOrEmpty(user.OAuthId)
            };

            return Ok(dto);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Некоректні дані профілю", details = ModelState });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { error = "Користувач не авторизований" });

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { error = "Некоректна сесія" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "Користувача не знайдено" });

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.Id != userId);
            if (emailExists)
                return BadRequest(new { error = "Користувач з таким email вже існує" });

            user.FullName = model.FullName.Trim();
            user.Phone = model.Phone.Trim();
            user.Email = model.Email.Trim();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Дані профілю успішно оновлено" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Некоректні дані", details = ModelState });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { error = "Користувач не авторизований" });

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { error = "Некоректна сесія" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "Користувача не знайдено" });

            user.Password = model.NewPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пароль успішно змінено" });
        }

        [HttpPost("unlink-google")]
        public async Task<IActionResult> UnlinkGoogleApi()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { error = "Користувач не авторизований" });

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { error = "Некоректна сесія" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "Користувача не знайдено" });

            user.OAuthId = null;
            user.OAuthProvider = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Google авторизацію відв’язано" });
        }

        [HttpGet("link-google")]
        public IActionResult LinkGoogle()
        {
            var redirectUrl = Url.Action(nameof(LinkGoogleCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("link-google-callback")]
        public async Task<IActionResult> LinkGoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded) return Redirect("http://localhost:5173/profile");

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var oauthId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Redirect("http://localhost:5173/profile");

            var user = await _context.Users.FindAsync(userId);
            if (user != null && email == user.Email)
            {
                user.OAuthId = oauthId;
                user.OAuthProvider = "Google";
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            Redirect("http://localhost:5173/profile");
            return Ok(new { message = "Google авторизацію прив’язано" });
        }
    }
}
