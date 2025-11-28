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
    public class SupplierControllerTests
    {
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
        public void GetSuppliers_ShouldReturnAllSuppliers()
        {
            // Arrange
            using var context = GetDatabaseContext();
            context.Suppliers.Add(new Supplier { Name = "S1", Email = "s1@t.com" });
            context.Suppliers.Add(new Supplier { Name = "S2", Email = "s2@t.com" });
            context.SaveChanges();

            var controller = new SupplierController(context);

            // Act
            var result = controller.GetSuppliers();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var suppliers = Assert.IsAssignableFrom<IEnumerable<Supplier>>(actionResult.Value);
            suppliers.Should().HaveCount(2);
        }

        [Fact]
        public void CreateSupplier_ShouldAddSupplier()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = new SupplierController(context);
            var newSupplier = new Supplier { Name = "New Supplier", Email = "new@supplier.com" };

            // Act
            var result = controller.CreateSupplier(newSupplier);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<Supplier>(actionResult.Value);
            created.Id.Should().BeGreaterThan(0);
        }
        
        // Podes adicionar testes para Update e Delete seguindo a mesma lógica dos outros ficheiros
    }
}