using Xunit;
using ProductsAPI.Controllers;
using ProductsAPI.Models;
using ProductsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System;

namespace ProductsAPI.Tests
{
    public class ProductsControllerTests
    {
        // Método auxiliar para criar uma BD "falsa" única para cada teste
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            using var context = GetDatabaseContext();
            context.Products.Add(new Product { Name = "P1", Price = 10M });
            context.Products.Add(new Product { Name = "P2", Price = 20M });
            context.SaveChanges();

            var controller = new ProductsController(context);

            // Act
            var result = controller.GetAll();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
            products.Should().HaveCount(2);
        }

        [Fact]
        public void GetById_WithExistingId_ShouldReturnProduct()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var product = new Product { Name = "Test Product", Price = 100.00M };
            context.Products.Add(product);
            context.SaveChanges();

            var controller = new ProductsController(context);

            // Act
            var result = controller.GetById(product.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<Product>(actionResult.Value);
            returnedProduct.Id.Should().Be(product.Id);
            returnedProduct.Name.Should().Be("Test Product");
        }

        [Fact]
        public void GetById_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = new ProductsController(context);

            // Act
            var result = controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ShouldAddProductAndReturnCreatedAtAction()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = new ProductsController(context);
            var newProduct = new Product { Name = "New Monitor", Price = 200.00M };

            // Act
            var result = controller.Create(newProduct);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProduct = Assert.IsType<Product>(actionResult.Value);
            createdProduct.Should().NotBeNull();
            createdProduct.Id.Should().BeGreaterThan(0);
            
            // Verificar se foi guardado na BD
            context.Products.Count().Should().Be(1);
        }

        [Fact]
        public void Update_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var product = new Product { Name = "Old Name", Price = 50.00M };
            context.Products.Add(product);
            context.SaveChanges();

            var controller = new ProductsController(context);
            var updatedProduct = new Product { Id = product.Id, Name = "New Name", Price = 75.00M };

            // Act
            var result = controller.Update(product.Id, updatedProduct);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verificar atualização
            var dbProduct = context.Products.Find(product.Id);
            dbProduct.Name.Should().Be("New Name");
        }

        [Fact]
        public void Delete_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var product = new Product { Name = "To Delete", Price = 50.00M };
            context.Products.Add(product);
            context.SaveChanges();

            var controller = new ProductsController(context);

            // Act
            var result = controller.Delete(product.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            context.Products.Count().Should().Be(0);
        }
    }
}