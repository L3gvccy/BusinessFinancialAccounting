using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessFinancialAccounting.Models;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для управління фінансовими звітами користувача.
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
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
        /// Показує головну сторінку звітів користувача.
        /// </summary>
        /// <returns>Представлення зі списком отриманих чеків та звітів користувача.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var receipts = await _context.Reciepts
                .Include(r => r.Products)
                .Where(r => r.User.Id == userId)
                .OrderByDescending(r => r.TimeStamp)
                .ToListAsync();

            var reports = await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            ViewBag.Receipts = receipts;
            ViewBag.Reports = reports;

            return View();
        }

        /// <summary>
        /// Генерує новий фінансовий звіт за вказаний період.
        /// </summary>
        /// <param name="startDate">Дата початку звітного періоду.</param>
        /// <param name="endDate">Дата кінця звітного періоду.</param>
        /// <returns>Redirect на перегляд згенерованого звіту.</returns>
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime startDate, DateTime endDate)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var receipts = await _context.Reciepts
                .Include(r => r.Products)
                .Where(r => r.User.Id == userId &&
                            r.TimeStamp >= startDate &&
                            r.TimeStamp <= endDate.AddDays(1))
                .ToListAsync();

            var ops = await _context.FinancialOperations
                .Where(f => f.User.Id == userId &&
                            f.TimeStamp >= startDate &&
                            f.TimeStamp <= endDate.AddDays(1))
                .ToListAsync();

            decimal cashSales = receipts
                .Where(r => r.PaymentMethod.ToLower() == "cash")
                .Sum(r => r.Products.Sum(p => p.TotalPrice));

            decimal cardSales = receipts
                .Where(r => r.PaymentMethod.ToLower() == "card")
                .Sum(r => r.Products.Sum(p => p.TotalPrice));

            decimal cashWithdrawals = ops.Sum(f => f.CashBalanceDecrease);
            decimal cardWithdrawals = ops.Sum(f => f.CardBalanceDecrease);

            decimal cashDeposits = ops.Sum(f => f.CashBalanceIncrease);
            decimal cardDeposits = ops.Sum(f => f.CardBalanceIncrease);

            decimal cashProfit = cashSales - cashWithdrawals;
            decimal cardProfit = cardSales - cardWithdrawals;

            decimal netIncome = cashProfit + cardProfit;
            decimal tax = Math.Round(netIncome * 0.195m, 2);

            var report = new Report
            {
                UserId = userId.Value,
                StartDate = startDate,
                EndDate = endDate,
                CashSales = cashSales,
                CardSales = cardSales,
                CashWithdrawals = cashWithdrawals,
                CardWithdrawals = cardWithdrawals,
                CashDeposits = cashDeposits,
                CardDeposits = cardDeposits,
                CashProfit = cashProfit,
                CardProfit = cardProfit,
                Tax = tax
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewReport", new { id = report.Id });
        }

        /// <summary>
        /// Переглядає конкретний звіт користувача.
        /// </summary>
        /// <param name="id">ID звіту.</param>
        /// <returns>Представлення звіту з деталями отриманих чеків.</returns>
        public async Task<IActionResult> ViewReport(int id)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            var receipts = await _context.Reciepts
                .Include(r => r.Products)
                .Where(r => r.User.Id == report.UserId &&
                            r.TimeStamp >= report.StartDate &&
                            r.TimeStamp <= report.EndDate.AddDays(1))
                .OrderBy(r => r.TimeStamp)
                .ToListAsync();

            ViewBag.Receipts = receipts;
            return View(report);
        }

        /// <summary>
        /// Повертає часткове представлення з деталями чеку.
        /// </summary>
        /// <param name="receiptId">ID чеку.</param>
        /// <returns>Часткове представлення _ReceiptDetails або NotFound.</returns>
        public async Task<IActionResult> GetReceiptDetails(int receiptId)
        {
            var receipt = await _context.Reciepts
                .Include(r => r.Products)
                .FirstOrDefaultAsync(r => r.Id == receiptId);

            if (receipt == null) return NotFound();

            return PartialView("_ReceiptDetails", receipt);
        }

        /// <summary>
        /// Показує список усіх звітів користувача.
        /// </summary>
        /// <returns>Представлення зі списком звітів.</returns>
        public async Task<IActionResult> ListReports()
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var reports = await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(reports);
        }

        /// <summary>
        /// Перегенеровує існуючий звіт, оновлюючи дані за тим же періодом.
        /// </summary>
        /// <param name="id">ID звіту для перегенерації.</param>
        /// <returns>Redirect на головну сторінку звітів після оновлення.</returns>
        [HttpPost]
        public async Task<IActionResult> RegenerateReport(int id)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (report == null) return NotFound();

            var receipts = await _context.Reciepts
                .Include(r => r.Products)
                .Where(r => r.User.Id == userId &&
                            r.TimeStamp >= report.StartDate &&
                            r.TimeStamp <= report.EndDate.AddDays(1))
                .ToListAsync();

            var ops = await _context.FinancialOperations
                .Where(f => f.User.Id == userId &&
                            f.TimeStamp >= report.StartDate &&
                            f.TimeStamp <= report.EndDate.AddDays(1))
                .ToListAsync();

            report.CashSales = receipts.Where(r => r.PaymentMethod.ToLower() == "cash").Sum(r => r.Products.Sum(p => p.TotalPrice));
            report.CardSales = receipts.Where(r => r.PaymentMethod.ToLower() == "card").Sum(r => r.Products.Sum(p => p.TotalPrice));
            report.CashWithdrawals = ops.Sum(f => f.CashBalanceDecrease);
            report.CardWithdrawals = ops.Sum(f => f.CardBalanceDecrease);
            report.CashDeposits = ops.Sum(f => f.CashBalanceIncrease);
            report.CardDeposits = ops.Sum(f => f.CardBalanceIncrease);
            report.CashProfit = report.CashSales - report.CashWithdrawals;
            report.CardProfit = report.CardSales - report.CardWithdrawals;
            report.Tax = Math.Round((report.CashProfit + report.CardProfit) * 0.195m, 2);

            await _context.SaveChangesAsync();

            TempData["AlertMsg"] = $"Звіт #{id} переформовано";
            TempData["AlertType"] = "success";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Видаляє звіт користувача.
        /// </summary>
        /// <param name="id">ID звіту для видалення.</param>
        /// <returns>Redirect на головну сторінку звітів після видалення.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var userId = TryGetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (report == null) return NotFound();

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["AlertMsg"] = $"Звіт #{id} видалено";
            TempData["AlertType"] = "success";

            return RedirectToAction("Index");
        }
    }
}

