using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessFinancialAccounting.Controllers;
using BusinessFinancialAccounting.Models;
using Microsoft.EntityFrameworkCore;
using static BusinessFinancialAccounting.Tests.ControllerTestSetup;

namespace BusinessFinancialAccounting.Tests
{
    public class ProductControllerTests
    {
        [Fact]
        public async Task AddProduct_InvalidModel_ReturnsView_WithDangerTempData()
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

            var controller = new ProductController(db)
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

            var product = new Product { Code = 0, Name = null, Units = "шт", Quantity = 0, Price = 0 };

            var result = await controller.AddProduct(product);
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(product, view.Model);
            Assert.Equal("danger", controller.TempData["AlertType"]);
        }

        [Fact]
        public async Task AddProduct_New_AddsAndRedirects()
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

            var controller = new ProductController(db)
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

            var product = new Product
            {
                Code = 111,
                Name = "Apple",
                Units = "шт",
                Quantity = 5,
                Price = 10
            };

            var result = await controller.AddProduct(product);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Products", redirect.ActionName);

            var saved = await db.Products.Include(p => p.User).SingleAsync();
            Assert.Equal("Apple", saved.Name);
            Assert.Equal(user.Id, saved.User.Id);
            Assert.Equal("success", controller.TempData["AlertType"]);
        }

        [Fact]
        public async Task AddProduct_Existing_IncreasesQuantity()
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

            var existing = new Product
            {
                Code = 222,
                Name = "Milk",
                Units = "кг",
                Quantity = 3,
                Price = 30,
                User = user
            };
            db.Products.Add(existing);
            db.SaveChanges();

            var controller = new ProductController(db)
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

            var add = new Product { Code = 222, Name = "Milk", Units = "л", Quantity = 2, Price = 30 };

            var result = await controller.AddProduct(add);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Products", redirect.ActionName);

            Assert.Equal(5, db.Products.Single().Quantity);
        }
    }
}
