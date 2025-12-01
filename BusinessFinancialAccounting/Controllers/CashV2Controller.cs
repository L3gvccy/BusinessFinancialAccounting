using Microsoft.AspNetCore.Mvc;

namespace BusinessFinancialAccounting.Controllers
{
    using BusinessFinancialAccounting.Models.DTO;
    using BusinessFinancialAccounting.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/v2/cash")]
    public class CashV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;

        public CashV2Controller(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Показує баланс користувача за рахунками готівки та картки.
        /// </summary>
        /// <returns>Інформацієя про баланс користувача.</returns>
        [HttpGet("cashregister")]
        public async Task<IActionResult> CashBalance()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Невірний UserId у сесії.");

            var cashRegister = await _context.CashRegisters
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.User.Id == userId);

            if (cashRegister == null)
            {
                cashRegister = new CashRegister { User = await _context.Users.FindAsync(userId), CardBalance = 0, CashBalance = 0 };
                _context.CashRegisters.Add(cashRegister);
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> TransactionForm([FromBody] TransactionDTO model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Невірний UserId у сесії.");

            var cashRegister = await _context.CashRegisters
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.User.Id == userId);

            if (cashRegister == null) return NotFound("Касовий рахунок не знайдено.");

            if (model.MoneyAmount <= 0) return BadRequest("Сума має бути більше нуля.");

            string message = string.Empty;

            if (model.ActionType == "withdraw")
            {
                if ((model.AccountType == "cash" && model.MoneyAmount > cashRegister.CashBalance) ||
                    (model.AccountType == "card" && model.MoneyAmount > cashRegister.CardBalance))
                {
                    return BadRequest(new { message = "Сума перевищує доступний баланс!" });
                }

                if (model.AccountType == "cash")
                {
                    cashRegister.CashBalance -= model.MoneyAmount;
                    message = $"Видано готівку: {model.MoneyAmount} грн. Новий баланс готівки: {cashRegister.CashBalance} грн.";
                }
                else
                {
                    cashRegister.CardBalance -= model.MoneyAmount;
                    message = $"Видано кошти з картки: {model.MoneyAmount} грн. Новий баланс картки: {cashRegister.CardBalance} грн.";
                }
            }
            else if (model.ActionType == "deposit")
            {
                if (model.AccountType == "cash")
                {
                    cashRegister.CashBalance += model.MoneyAmount;
                    message = $"Прийнято готівку: {model.MoneyAmount} грн. Новий баланс готівки: {cashRegister.CashBalance} грн.";
                }
                else
                {
                    cashRegister.CardBalance += model.MoneyAmount;
                    message = $"Прийнято кошти на картку: {model.MoneyAmount} грн. Новий баланс картки: {cashRegister.CardBalance} грн.";
                }
            }
            else
            {
                return BadRequest("Невідомий тип операції.");
            }

            var operation = new FinancialOperation
            {
                User = cashRegister.User,
                CashBalanceIncrease = model.AccountType == "cash" && model.ActionType == "deposit" ? (int)model.MoneyAmount : 0,
                CashBalanceDecrease = model.AccountType == "cash" && model.ActionType == "withdraw" ? (int)model.MoneyAmount : 0,
                CardBalanceIncrease = model.AccountType == "card" && model.ActionType == "deposit" ? (int)model.MoneyAmount : 0,
                CardBalanceDecrease = model.AccountType == "card" && model.ActionType == "withdraw" ? (int)model.MoneyAmount : 0,
                TimeStamp = DateTime.UtcNow
            };

            _context.FinancialOperations.Add(operation);
            await _context.SaveChangesAsync();

            return Ok(new { message });
        }
    }

}
