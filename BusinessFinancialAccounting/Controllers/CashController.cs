using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace BusinessFinancialAccounting.Controllers
{
    public class CashController : Controller
    {
        private AppDbContext _context;

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

        [HttpPost]
        public IActionResult UpdateBalance(int cashIncrease, int cashDecrease, int cardIncrease, int cardDecrease)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.User.Id == userId);
            if (cashRegister == null) return RedirectToAction("CashRegister");

            cashRegister.CashBalance += cashIncrease - cashDecrease;
            cashRegister.CardBalance += cardIncrease - cardDecrease;

            var operation = new FinancialOperation
            {
                User = _context.Users.Find(userId),
                CashBalanceIncrease = cashIncrease,
                CashBalanceDecrease = cashDecrease,
                CardBalanceIncrease = cardIncrease,
                CardBalanceDecrease = cardDecrease,
                TimeStamp = DateTime.Now 
            };

            _context.FinancialOperations.Add(operation);
            _context.SaveChanges();

            return RedirectToAction("CashBalance");
        }
    }
}
