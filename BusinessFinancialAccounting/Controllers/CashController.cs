using BusinessFinancialAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using BusinessFinancialAccounting.Models.DTO;

namespace BusinessFinancialAccounting.Controllers
{
    /// <summary>
    /// Контролер для управління грошовими операціями користувача.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CashController : Controller
    {
        private readonly AppDbContext _context;

        public CashController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Показує баланс користувача за рахунками готівки та картки.
        /// </summary>
        /// <returns>Інформацієя про баланс користувача.</returns>
        [HttpGet("cashregister")]
        public IActionResult CashBalance()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return Unauthorized();

            int userId = int.Parse(userIdStr);
            var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.User.Id == userId);

            if (cashRegister != null)
            {
            }
            else
            {
                cashRegister = new CashRegister { CardBalance = 0, CashBalance = 0 };
                _context.CashRegisters.Add(cashRegister);
                _context.SaveChanges();
            }

            return Ok(new { cash = cashRegister.CashBalance, card = cashRegister.CardBalance });
        }

        /// <summary>
        /// Обробляє фінансову операцію користувача: внесення або видачу коштів.
        /// </summary>
        /// <param name="model">Модель з даними операції.</param>
        /// <returns>
        /// Повідомлення про успішність операції або помилку, якщо коштів недостатньо.
        /// </returns>
        [HttpPost("transaction")]
        public IActionResult TransactionForm(TransactionDTO model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return Unauthorized();

            int userId = int.Parse(userIdStr);
            var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.User.Id == userId);
            if (cashRegister == null) return BadRequest();
            var message = "";

            if (model.ActionType == "withdraw")
            {
                bool notEnough = (model.AccountType == "cash" && model.MoneyAmount > cashRegister.CashBalance)
                              || (model.AccountType == "card" && model.MoneyAmount > cashRegister.CardBalance);

                

                if (notEnough)
                {
                    return BadRequest(new { message = "Сума перевищує доступний баланс!" });
                }

                if (model.AccountType == "cash")
                {
                    cashRegister.CashBalance -= model.MoneyAmount;
                    message = $"Видача готівки: {model.MoneyAmount} грн.";
                }
                else
                {
                    cashRegister.CardBalance -= model.MoneyAmount;
                    message = $"Видача карткою: {model.MoneyAmount} грн.";
                }
            }
            else if (model.ActionType == "deposit")
            {
                if (model.AccountType == "cash")
                {
                    cashRegister.CashBalance += model.MoneyAmount;
                    message = $"Внесення готівкою: {model.MoneyAmount} грн.";
                } 
                else
                {
                    cashRegister.CardBalance += model.MoneyAmount;
                    message = $"Внесення карткою: {model.MoneyAmount} грн.";
                }
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
            return Ok(new { message = message});
        }
    }
}
