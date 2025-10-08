using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessFinancialAccounting.Controllers;
using BusinessFinancialAccounting.Models;
using static BusinessFinancialAccounting.Tests.ControllerTestSetup;

namespace BusinessFinancialAccounting.Tests
{
    public class SaleControllerTests
    {
        [Fact]
        public async Task AddByCode_ProductNotFound_SetsDangerAndRedirects()
        {
            using var db = TestDb.CreateContext();

            var user = new User
            {
                Username = "u",
                Password = "p",
                FullName = "User",
                Email = "u@example.com",
                Phone = "+3800000000"
            };
            db.Users.Add(user);
            db.SaveChanges();

            var controller = new SaleController(db)
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

            var result = await controller.AddByCode(999);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Sale", redirect.ActionName);
            Assert.Equal("danger", controller.TempData["AlertType"]);
        }

        [Fact]
        public async Task ChangeQty_CapsToStock_AndRoundsByUnits()
        {
            using var db = TestDb.CreateContext();

            var user = new User
            {
                Username = "u",
                Password = "p",
                FullName = "User",
                Email = "u@example.com",
                Phone = "+3800000000"
            };
            var p = new Product
            {
                Code = 5,
                Name = "Cheese",
                Units = "КГ",
                Quantity = 2.000m,
                Price = 200m,
                User = user
            };
            db.Users.Add(user);
            db.Products.Add(p);
            db.SaveChanges();

            var controller = new SaleController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = HttpWithSession(new Dictionary<string, string>
                    {
                        ["UserId"] = user.Id.ToString(),
                        ["POS_CART_QTY"] = "{}"
                    })
                }
            };
            WireTempData(controller);

            var result = await controller.ChangeQty(p.Id, "3.14159");
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Sale", redirect.ActionName);
            Assert.Equal("warning", controller.TempData["AlertType"]); 

            var json = controller.HttpContext.Session.GetString("POS_CART_QTY");
            Assert.Contains($"\"{p.Id}\":2.0", json);
        }

        [Fact]
        public async Task Pay_Cash_CreatesReceipt_UpdatesBalances_AndClearsCart()
        {
            using var db = TestDb.CreateContext();

            var user = new User
            {
                Username = "u",
                Password = "p",
                FullName = "User",
                Email = "u@example.com",
                Phone = "+3800000000"
            };
            var prod = new Product
            {
                Code = 10,
                Name = "Tea",
                Units = "ШТ",
                Quantity = 10,
                Price = 25m,
                User = user
            };
            var reg = new CashRegister { User = user, CashBalance = 0, CardBalance = 0 };
            db.Users.Add(user);
            db.Products.Add(prod);
            db.CashRegisters.Add(reg);
            db.SaveChanges();

            var cart = new Dictionary<int, decimal> { [prod.Id] = 2 };
            var http = HttpWithSession(new Dictionary<string, string>
            {
                ["UserId"] = user.Id.ToString(),
                ["POS_CART_QTY"] = System.Text.Json.JsonSerializer.Serialize(cart)
            });

            var controller = new SaleController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = http }
            };
            WireTempData(controller);

            var result = await controller.Pay("cash");
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Sale", redirect.ActionName);

            Assert.Single(db.Reciepts);
            Assert.Single(db.ReceiptProducts);
            Assert.Equal(2, db.ReceiptProducts.Single().Quantity);

            Assert.Equal(50, db.CashRegisters.Single().CashBalance);
            Assert.Equal(8, db.Products.Single().Quantity);

            Assert.Equal("{}", controller.HttpContext.Session.GetString("POS_CART_QTY"));
            Assert.Equal("success", controller.TempData["AlertType"]);
        }
    }
}
