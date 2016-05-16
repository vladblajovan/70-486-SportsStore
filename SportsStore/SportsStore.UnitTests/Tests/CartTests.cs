using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // Arrange
            Product p1 = new Product
            {
                ProductID = 1,
                Name = "P1"
            };

            Product p2 = new Product
            {
                ProductID = 2,
                Name = "P2"
            };

            Cart cart = new Cart();

            // Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            CartLine[] result = cart.Lines.ToArray();

            // Assert
            Assert.AreEqual(result.Length, 2);
            Assert.AreEqual(result[0].Product, p1);
            Assert.AreEqual(result[1].Product, p2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // Arrange
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            Cart cart = new Cart();

            // Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            cart.AddItem(p1, 10);
            CartLine[] result = cart.Lines.OrderBy(c => c.Product.ProductID).ToArray();

            // Assert
            Assert.AreEqual(result.Length, 2);
            Assert.AreEqual(result[0].Quantity, 11);
            Assert.AreEqual(result[1].Quantity, 1);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            // Arrange
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };
            Cart cart = new Cart();

            // Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 3);
            cart.AddItem(p3, 5);
            cart.AddItem(p2, 1);
            cart.RemoveLine(p2);

            // Assert
            Assert.AreEqual(cart.Lines.Where(c => c.Product == p2).Count(), 0);
            Assert.AreEqual(cart.Lines.Count(), 2);
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // Arrange
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            Cart cart = new Cart();

            // Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            cart.AddItem(p1, 3);
            decimal result = cart.ComputeTotalValue();

            // Assert
            Assert.AreEqual(result, 450M);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            // Arrange
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            Cart cart = new Cart();

            // Act
            cart.AddItem(p1, 1);
            cart.AddItem(p2, 1);
            cart.Clear();

            // Assert
            Assert.AreEqual(cart.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product { ProductID = 1, Name = "P1", Category = "Apples" }
            }.AsQueryable());
            Cart cart = new Cart();
            CartController target = new CartController(mock.Object, null);

            // Act
            target.AddToCart(cart, 1, null);

            // Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product { ProductID = 1, Name = "P1", Category = "Apples" }
            }.AsQueryable());

            Cart cart = new Cart();
            CartController target = new CartController(mock.Object, null);

            // Act
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            // Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // Arrange
            Cart cart = new Cart();
            CartController target = new CartController(null, null);

            // Act
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // Assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }
    }
}
 