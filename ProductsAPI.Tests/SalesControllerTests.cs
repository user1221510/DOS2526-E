using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsAPI.Controllers;
using ProductsAPI.Data;
using ProductsAPI.Models;
using Xunit;

namespace ProductsAPI.Tests
{
    public class SalesControllerTests
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
        public void Create_ShouldAddSale_WhenUserExists()
        {
            using var context = GetDatabaseContext();
            
            // 1. Criar o User (Dados Reais para o teste)
            var user = new User { Username = "joao", Email = "j@t.com", FullName = "Joao", Role = "Admin" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = new SalesController(context);
            var newSale = new Sale 
            { 
                Description = "New Sale", 
                TotalPrice = 150.50M, 
                UserID = user.Id
                // Nota: No teste Create não definimos o objeto User manualmente porque queremos 
                // testar se o Controller é capaz de o ir buscar à BD sozinho.
            };

            // Act
            var result = controller.Create(newSale);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSale = Assert.IsType<Sale>(actionResult.Value);
            
            createdSale.Should().NotBeNull();
            createdSale.User.Should().NotBeNull();
            createdSale.User!.Id.Should().Be(user.Id);
        }

        [Fact]
        public void GetSales_ShouldReturnAllSales()
        {
            using var context = GetDatabaseContext();
            
            // 1. Criar User
            var user = new User { Username = "maria", Email = "m@t.com", FullName = "Maria", Role = "User" };
            context.Users.Add(user);
            context.SaveChanges();
            
            // 2. Criar Venda ligada ao User
            // IMPORTANTE: Definimos 'User = user' explicitamente para garantir que o EF Core In-Memory
            // faz a ligação imediata para este teste de leitura (Get).
            context.Sales.Add(new Sale { 
                Description = "S1", 
                TotalPrice = 10M, 
                UserID = user.Id,
                User = user 
            });
            context.SaveChanges();

            var controller = new SalesController(context);

            // Act
            var result = controller.GetSales();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var sales = Assert.IsAssignableFrom<IEnumerable<Sale>>(actionResult.Value);
            
            // Agora deve encontrar 1 item com certeza
            sales.Should().HaveCount(1);
        }

        [Fact]
        public void GetSale_WithExistingId_ShouldReturnSale()
        {
            using var context = GetDatabaseContext();
            
            // 1. Criar User
            var user = new User { Username = "pedro", Email = "p@t.com", FullName = "Pedro", Role = "User" };
            context.Users.Add(user);
            context.SaveChanges();

            // 2. Criar Venda ligada ao User
            var sale = new Sale { 
                Description = "Test Sale", 
                TotalPrice = 100M, 
                UserID = user.Id,
                User = user 
            };
            context.Sales.Add(sale);
            context.SaveChanges();

            var controller = new SalesController(context);

            // Act
            var result = controller.GetSale(sale.Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Sale>(actionResult.Value);
            returned.Id.Should().Be(sale.Id);
        }
    }
}