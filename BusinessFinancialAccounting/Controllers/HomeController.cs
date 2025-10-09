using System.Diagnostics;
using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для головної сторінки застосунку та стандартних системних сторінок.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Показує головну сторінку застосунку.
        /// </summary>
        /// <returns>Представлення головної сторінки.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Показує сторінку з політикою конфіденційності.
        /// </summary>
        /// <returns>Представлення сторінки Privacy.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Показує сторінку помилки.
        /// </summary>
        /// <returns>Представлення сторінки помилки.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
