using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace BusinessFinancialAccounting.Controllers
{
    public class CashController : Controller
    {
        private readonly AppDbContext _context;

        public CashController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult CashBalance()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);
            var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.User.Id == userId);

            return View(cashRegister);
        }

        // Відображення модального вікна
        [HttpGet]
        public IActionResult TransactionForm(string accountType, string actionType)
        {
            var model = new TransactionViewModel
            {
                AccountType = accountType,
                ActionType = actionType
            };

            return PartialView("_TransactionForm", model);
        }

        // Обробка введеної суми
        [HttpPost]
        public IActionResult TransactionForm(TransactionViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);
            var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.User.Id == userId);
            if (cashRegister == null) return NotFound();

            if (model.ActionType == "withdraw")
            {
                bool notEnough = (model.AccountType == "cash" && model.MoneyAmount > cashRegister.CashBalance)
                              || (model.AccountType == "card" && model.MoneyAmount > cashRegister.CardBalance);

                if (notEnough)
                {
                    model.ErrorMessage = "Сума перевищує доступний баланс!";
                    return PartialView("_TransactionForm", model);
                }

                if (model.AccountType == "cash")
                    cashRegister.CashBalance -= model.MoneyAmount;
                else
                    cashRegister.CardBalance -= model.MoneyAmount;
            }
            else if (model.ActionType == "deposit")
            {
                if (model.AccountType == "cash")
                    cashRegister.CashBalance += model.MoneyAmount;
                else
                    cashRegister.CardBalance += model.MoneyAmount;
            }

            var operation = new FinancialOperation
            {
                User = _context.Users.Find(userId),
                CashBalanceIncrease = (model.AccountType == "cash" && model.ActionType == "deposit") ? (int)model.MoneyAmount : 0,
                CashBalanceDecrease = (model.AccountType == "cash" && model.ActionType == "withdraw") ? (int)model.MoneyAmount : 0,
                CardBalanceIncrease = (model.AccountType == "card" && model.ActionType == "deposit") ? (int)model.MoneyAmount : 0,
                CardBalanceDecrease = (model.AccountType == "card" && model.ActionType == "withdraw") ? (int)model.MoneyAmount : 0,
                TimeStamp = DateTime.Now
            };

            _context.FinancialOperations.Add(operation);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
