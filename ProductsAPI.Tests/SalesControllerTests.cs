using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Controllers;
using ProductsAPI.Models;
using Xunit;

namespace ProductsAPI.Tests
{
    public class SalesControllerTests
    {
        [Fact]
        public void GetSales_ShouldReturnAllSales()
        {
            // Arrange
            var controller = new SalesController();

            // Act
            var result = controller.GetSales();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var sales = Assert.IsAssignableFrom<IEnumerable<Sale>>(actionResult.Value);
            sales.Should().NotBeNull();
            Assert.True(sales.Count() >= 1); // Pelo menos a venda inicial existe
        }

        [Fact]
        public void GetSale_WithExistingId_ShouldReturnSale()
        {
            // Arrange
            var controller = new SalesController();
            var expectedSaleId = 1;

            // Act
            var result = controller.GetSale(expectedSaleId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var sale = Assert.IsType<Sale>(actionResult.Value);
            sale.Id.Should().Be(expectedSaleId);
            sale.Description.Should().NotBeNullOrEmpty();
            sale.TotalPrice.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetSale_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SalesController();
            var nonExistingId = 999;

            // Act
            var result = controller.GetSale(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ShouldAddSaleAndReturnCreatedAtAction()
        {
            // Arrange
            var controller = new SalesController();
            var newSale = new Sale 
            { 
                Description = "New Test Sale", 
                TotalPrice = 150.50, 
                Products = new List<Product>(),
                UserID = 1
            };

            // Act
            var result = controller.Create(newSale);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSale = Assert.IsType<Sale>(actionResult.Value);
            createdSale.Should().NotBeNull();
            createdSale.Id.Should().BeGreaterThan(0);
            createdSale.Description.Should().Be(newSale.Description);
            createdSale.TotalPrice.Should().Be(newSale.TotalPrice);
            createdSale.UserID.Should().Be(newSale.UserID);
        }

        [Fact]
        public void Create_WithNullProducts_ShouldInitializeEmptyProductsList()
        {
            // Arrange
            var controller = new SalesController();
            var newSale = new Sale 
            { 
                Description = "Sale with Null Products", 
                TotalPrice = 100.00, 
                Products = null, // Explicitly set to null
                UserID = 1
            };

            // Act
            var result = controller.Create(newSale);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSale = Assert.IsType<Sale>(actionResult.Value);
            createdSale.Products.Should().NotBeNull();
            createdSale.Products.Should().BeEmpty();
        }

        [Fact]
        public void Update_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new SalesController();
            var saleIdToUpdate = 1;
            var updatedSale = new Sale 
            { 
                Id = saleIdToUpdate, 
                Description = "Updated Sale Description", 
                TotalPrice = 200.75,
                Products = new List<Product>(),
                UserID = 1
            };

            // Act
            var result = controller.Update(saleIdToUpdate, updatedSale);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SalesController();
            var nonExistingId = 999;
            var updatedSale = new Sale 
            { 
                Id = nonExistingId, 
                Description = "Updated Sale", 
                TotalPrice = 200.00,
                Products = new List<Product>(),
                UserID = 1
            };

            // Act
            var result = controller.Update(nonExistingId, updatedSale);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Update_ShouldModifySaleProperties()
        {
            // Arrange
            var controller = new SalesController();
            var saleIdToUpdate = 1;
            
            // First get the original sale
            var originalResult = controller.GetSale(saleIdToUpdate);
            var originalSale = (Sale)((OkObjectResult)originalResult.Result).Value;

            var updatedSale = new Sale 
            { 
                Id = saleIdToUpdate, 
                Description = "Completely Updated Sale", 
                TotalPrice = 300.99,
                Products = new List<Product> 
                { 
                    new Product { Id = 1, Name = "Updated Product", Price = 99.99M } 
                },
                UserID = 2
            };

            // Act
            var updateResult = controller.Update(saleIdToUpdate, updatedSale);
            var getResult = controller.GetSale(saleIdToUpdate);
            var retrievedSale = (Sale)((OkObjectResult)getResult.Result).Value;

            // Assert
            Assert.IsType<NoContentResult>(updateResult);
            retrievedSale.Description.Should().Be("Completely Updated Sale");
            retrievedSale.TotalPrice.Should().Be(300.99);
            retrievedSale.Products.Should().HaveCount(1);
            retrievedSale.UserID.Should().Be(2);
        }

        [Fact]
        public void Delete_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new SalesController();
            
            // First create a sale to delete
            var newSale = new Sale 
            { 
                Description = "Sale to Delete", 
                TotalPrice = 50.00, 
                Products = new List<Product>(),
                UserID = 1
            };
            var createResult = controller.Create(newSale);
            var createdSale = (Sale)((CreatedAtActionResult)createResult.Result).Value;

            // Act
            var result = controller.Delete(createdSale.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify the sale is actually deleted
            var getResult = controller.GetSale(createdSale.Id);
            Assert.IsType<NotFoundResult>(getResult.Result);
        }

        [Fact]
        public void Delete_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SalesController();
            var nonExistingId = 999;

            // Act
            var result = controller.Delete(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_WithValidUserID_ShouldAssignUser()
        {
            // Arrange
            var controller = new SalesController();
            var newSale = new Sale 
            { 
                Description = "Sale with User", 
                TotalPrice = 250.00, 
                Products = new List<Product>(),
                UserID = 1 // Valid user ID from mock data
            };

            // Act
            var result = controller.Create(newSale);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSale = Assert.IsType<Sale>(actionResult.Value);
            createdSale.User.Should().NotBeNull();
            createdSale.User.Id.Should().Be(1);
            createdSale.User.Username.Should().Be("joaos");
        }

        [Fact]
        public void Create_WithInvalidUserID_ShouldStillCreateSale()
        {
            // Arrange
            var controller = new SalesController();
            var newSale = new Sale 
            { 
                Description = "Sale with Invalid User", 
                TotalPrice = 150.00, 
                Products = new List<Product>(),
                UserID = 999 // Invalid user ID
            };

            // Act
            var result = controller.Create(newSale);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSale = Assert.IsType<Sale>(actionResult.Value);
            createdSale.Should().NotBeNull();
            createdSale.Id.Should().BeGreaterThan(0);
            // Note: In the current implementation, invalid UserID still creates the sale
            // but the User property might be null or have mock data
        }
    }
}