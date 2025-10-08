using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using BusinessFinancialAccounting.Controllers;
using BusinessFinancialAccounting.Models;
using static BusinessFinancialAccounting.Tests.ControllerTestSetup;

namespace BusinessFinancialAccounting.Tests
{
    public class CashControllerTests
    {
        [Fact]
        public void CashBalance_NoSession_RedirectsToLogin()
        {
            using var db = TestDb.CreateContext();
            var controller = new CashController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = HttpWithSession() }
            };

            var result = controller.CashBalance();
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        [Fact]
        public void Transaction_Deposit_Cash_UpdatesBalance_And_WritesOperation()
        {
            using var db = TestDb.CreateContext();

            var user = new User
            {
                Username = "u1",
                Password = "p",
                FullName = "User One",
                Email = "u1@example.com",
                Phone = "+3800000001"
            };

            db.Users.Add(user);
            var reg = new CashRegister { User = user, CashBalance = 100, CardBalance = 0 };
            db.CashRegisters.Add(reg);
            db.SaveChanges(); 

            var controller = new CashController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = HttpWithSession(new Dictionary<string, string>
                    {
                        ["UserId"] = user.Id.ToString()
                    })
                }
            };
            WireTempData(controller);

            var model = new TransactionViewModel
            {
                AccountType = "cash",
                ActionType = "deposit",
                MoneyAmount = 50
            };

            var result = controller.TransactionForm(model);

            var json = Assert.IsType<JsonResult>(result);

            var successProp = json.Value?.GetType().GetProperty("success");
            var successVal = successProp?.GetValue(json.Value);
            var successStr = successVal?.ToString();
            Assert.Equal("true", successStr);

            Assert.Equal(150, db.CashRegisters.Single().CashBalance);

            Assert.Single(db.FinancialOperations);
            var op = db.FinancialOperations.Single();
            Assert.Equal(50, op.CashBalanceIncrease);
            Assert.Equal(0, op.CashBalanceDecrease);
            Assert.Equal(0, op.CardBalanceIncrease);
            Assert.Equal(0, op.CardBalanceDecrease);
        }

        [Fact]
        public void Transaction_Withdraw_NotEnough_ReturnsPartialViewWithError()
        {
            using var db = TestDb.CreateContext();

            var user = new User
            {
                Username = "u1",
                Password = "p",
                FullName = "User One",
                Email = "u1@example.com",
                Phone = "+3800000001"
            };

            db.Users.Add(user);
            db.CashRegisters.Add(new CashRegister { User = user, CashBalance = 10, CardBalance = 0 });
            db.SaveChanges();

            var controller = new CashController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = HttpWithSession(new Dictionary<string, string>
                    {
                        ["UserId"] = user.Id.ToString()
                    })
                }
            };
            WireTempData(controller);

            var model = new TransactionViewModel
            {
                AccountType = "cash",
                ActionType = "withdraw",
                MoneyAmount = 50
            };

            var result = controller.TransactionForm(model);
            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_TransactionForm", pv.ViewName);
            var returned = Assert.IsType<TransactionViewModel>(pv.Model);
            Assert.Contains("перевищує", returned.ErrorMessage); 
        }
    }
}
