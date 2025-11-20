using Xunit;
using ProductsAPI.Controllers;
using ProductsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace ProductsAPI.Tests
{
    public class UserControllerTests
    {
        [Fact]
        public void GetUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var controller = new UsersController();

            // Act
            var result = controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(actionResult.Value);
            users.Should().NotBeNull();
        }

        [Fact]
        public void GetUsers_WithRoleFilter_ShouldReturnFilteredUsers()
        {
            // Arrange
            var controller = new UsersController();
            var role = "Admin";

            // Act
            var result = controller.GetUsers(role);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(actionResult.Value);
            users.Should().NotBeNull();
        }

        [Fact]
        public void GetUser_WithExistingId_ShouldReturnUser()
        {
            // Arrange
            var controller = new UsersController();
            var expectedUserId = 1;

            // Act
            var result = controller.GetUser(expectedUserId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(actionResult.Value);
            user.Id.Should().Be(expectedUserId);
        }

        [Fact]
        public void GetUser_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new UsersController();
            var nonExistingId = 999;

            // Act
            var result = controller.GetUser(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void CreateUser_ShouldAddUserAndReturnCreatedAtAction()
        {
            // Arrange
            var controller = new UsersController();
            var newUser = new User 
            { 
                Username = "newuser", 
                Email = "newuser@example.com", 
                FullName = "New User", 
                Role = "User" 
            };

            // Act
            var result = controller.CreateUser(newUser);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdUser = Assert.IsType<User>(actionResult.Value);
            createdUser.Should().NotBeNull();
            createdUser.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void UpdateUser_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new UsersController();
            var userIdToUpdate = 1;
            var updatedUser = new User 
            { 
                Id = userIdToUpdate, 
                Username = "updateduser", 
                Email = "updated@email.com", 
                FullName = "Updated Name", 
                Role = "Admin" 
            };

            // Act
            var result = controller.UpdateUser(userIdToUpdate, updatedUser);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void UpdateUser_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new UsersController();
            var nonExistingId = 999;
            var updatedUser = new User 
            { 
                Id = nonExistingId, 
                Username = "updateduser", 
                Email = "updated@email.com", 
                FullName = "Updated Name", 
                Role = "Admin" 
            };

            // Act
            var result = controller.UpdateUser(nonExistingId, updatedUser);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteUser_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new UsersController();
            var userIdToDelete = 1;

            // Act
            var result = controller.DeleteUser(userIdToDelete);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteUser_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new UsersController();
            var nonExistingId = 999;

            // Act
            var result = controller.DeleteUser(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}