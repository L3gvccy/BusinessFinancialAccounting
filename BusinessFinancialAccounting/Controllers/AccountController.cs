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
        /// <returns>Повідомлення про успіх чи помилку при виконанні запиту</returns>
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
        /// <returns>Повідомлення про успіх чи помилку при виконанні запиту</returns>
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
        /// <returns>Повідомлення про успіх при виконанні запиту</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { alertMsg = "Ви успішно вийши з акаунуту!", alertType = "success" });
        }

        /// <summary>
        /// Вхід через зовнішнього провайдера
        /// </summary>
        /// <param name="provider">Провайдер для OAUTH-2</param>
        /// <returns>Виклик фкнкції обробки автентифікація</returns>
        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, provider);
        }

        /// <summary>
        /// Автентифікація через зовнішнього провайдера
        /// </summary>
        /// <param name="provider">Провайдер для OAUTH-2</param>
        /// <returns>Редірект на головну сторінку з кодом повідомлення</returns>
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string provider = "Google")
        {
            var result = await HttpContext.AuthenticateAsync(provider);
            if (!result.Succeeded)
                return Redirect("http://localhost:5173/login?error=google_auth_failed");

            var claims = result.Principal?.Claims;
            if (claims == null)
                return Redirect("http://localhost:5173/login?error=google_no_claims");

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var oauthId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oauthId))
                return Redirect("http://localhost:5173/login?error=google_missing_data");

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

            return Redirect("http://localhost:5173/");
        }

        /// <summary>
        /// Отримує профіль користувача
        /// </summary>
        /// <returns>Дані користувача</returns>
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

        /// <summary>
        /// Оновлює дані профілю користувача
        /// </summary>
        /// <param name="model">DTO для зміни даних користувача</param>
        /// <returns>Повідомлення про успіх чи помилку при виконанні запиту</returns>
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

        /// <summary>
        /// Змінює пароль користувача
        /// </summary>
        /// <param name="model">DTO для зміни паролю</param>
        /// <returns>Повідомлення про успіх чи помилку при виконанні запиту</returns>
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

        /// <summary>
        /// Відв’язує обліковий запис Google від профілю користувача
        /// </summary>
        /// <returns>Повідомлення про успіх чи помилку виконання запиту</returns>
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

        /// <summary>
        /// Приймає запит на зв'язування облікового запису Google
        /// </summary>
        /// <returns>Викли функції обробки прив'язки облікового запису Google</returns>
        [HttpGet("link-google")]
        public IActionResult LinkGoogle()
        {
 
            var redirectUrl = Url.Action(nameof(LinkGoogleCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Обробляє зворотній виклик для зв'язування облікового запису Google
        /// </summary>
        /// <returns>Редірект на сторінку профілю з кодом повідомлення</returns>
        [HttpGet("link-google-callback")]
        public async Task<IActionResult> LinkGoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
                return Redirect("http://localhost:5173/profile?error=google_auth_failed");

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var oauthId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Redirect("http://localhost:5173/profile?error=session_expired");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Redirect("http://localhost:5173/profile?error=user_not_found");

            user.OAuthId = oauthId;
            user.OAuthProvider = "Google";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Redirect("http://localhost:5173/profile?success=google_linked");
        }

    }
}
