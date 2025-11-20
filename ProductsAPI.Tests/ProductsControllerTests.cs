using Xunit;
using ProductsAPI.Controllers;
using ProductsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace ProductsAPI.Tests
{
    public class ProductsControllerTests
    {
        [Fact]
        public void GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            var controller = new ProductsController();

            // Act
            var result = controller.GetAll();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
            products.Should().NotBeNull();
        }

        [Fact]
        public void GetById_WithExistingId_ShouldReturnProduct()
        {
            // Arrange
            var controller = new ProductsController();
            
            // Primeiro cria um produto para garantir que existe
            var newProduct = new Product { Name = "Test Product", Price = 100.00M };
            var createResult = controller.Create(newProduct);
            var createdProduct = (Product)((CreatedAtActionResult)createResult.Result).Value;

            // Act
            var result = controller.GetById(createdProduct.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(actionResult.Value);
            product.Id.Should().Be(createdProduct.Id);
            product.Name.Should().Be("Test Product");
        }

        [Fact]
        public void GetById_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new ProductsController();
            var nonExistingId = 999;

            // Act
            var result = controller.GetById(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ShouldAddProductAndReturnCreatedAtAction()
        {
            // Arrange
            var controller = new ProductsController();
            var newProduct = new Product { Name = "New Monitor", Price = 200.00M };

            // Act
            var result = controller.Create(newProduct);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProduct = Assert.IsType<Product>(actionResult.Value);
            createdProduct.Should().NotBeNull();
            createdProduct.Id.Should().BeGreaterThan(0);
            createdProduct.Name.Should().Be(newProduct.Name);
            createdProduct.Price.Should().Be(newProduct.Price);
        }

        [Fact]
        public void Update_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new ProductsController();
            
            // Primeiro cria um produto
            var newProduct = new Product { Name = "Product to Update", Price = 50.00M };
            var createResult = controller.Create(newProduct);
            var createdProduct = (Product)((CreatedAtActionResult)createResult.Result).Value;

            var updatedProduct = new Product { Id = createdProduct.Id, Name = "Updated Product", Price = 75.00M };

            // Act
            var result = controller.Update(createdProduct.Id, updatedProduct);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new ProductsController();
            var nonExistingId = 999;
            var updatedProduct = new Product { Id = nonExistingId, Name = "Updated Product", Price = 75.00M };

            // Act
            var result = controller.Update(nonExistingId, updatedProduct);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new ProductsController();
            
            // Primeiro cria um produto
            var newProduct = new Product { Name = "Product to Delete", Price = 50.00M };
            var createResult = controller.Create(newProduct);
            var createdProduct = (Product)((CreatedAtActionResult)createResult.Result).Value;

            // Act
            var result = controller.Delete(createdProduct.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new ProductsController();
            var nonExistingId = 999;

            // Act
            var result = controller.Delete(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}