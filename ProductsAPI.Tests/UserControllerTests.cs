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
    public class UserControllerTests
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
        public void GetUsers_ShouldReturnAllUsers()
        {
            // Arrange
            using var context = GetDatabaseContext();
            context.Users.Add(new User { Username = "u1", Email = "u1@t.com", FullName="U1", Role="User" });
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act
            var result = controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(actionResult.Value);
            users.Should().HaveCount(1);
        }

        [Fact]
        public void CreateUser_ShouldAddUser()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = new UsersController(context);

            // CORREÇÃO: Usamos o tipo que o Controller espera (UserCreateRequest)
            // Se o compilador reclamar que UserCreateRequest não existe, 
            // garante que tens 'using ProductsAPI.Controllers;' lá em cima.
            var newUserRequest = new UserCreateRequest 
            { 
                Username = "new", 
                Email = "new@t.com", 
                FullName = "New", 
                Role = "Admin" 
            };

            // Act
            var result = controller.CreateUser(newUserRequest);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<User>(actionResult.Value);
            
            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.Username.Should().Be("new");
        }
    }
}