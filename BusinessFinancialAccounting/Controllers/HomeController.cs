using System.Diagnostics;
using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для головної сторінки застосунку та стандартних системних сторінок.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { message = "App works!" });
        }

        /// <summary>
        /// Отримує інформацію про поточного автентифікованого користувача.
        /// </summary>
        /// <returns>
        /// Статус автентифікації та дані користувача, якщо він автентифікований.
        /// </returns>
        [HttpGet("me")]
        public IActionResult GetUser()
        {
            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetString("UserId");

            if (username == null)
                return Unauthorized(new { isAuthenticated = false });

            return Ok(new { isAuthenticated = true, username, userId });
        }
    }
}
