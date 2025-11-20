using Xunit;
using ProductsAPI.Controllers;
using ProductsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace ProductsAPI.Tests
{
    public class SupplierControllerTests
    {
        [Fact]
        public void GetSuppliers_ShouldReturnAllSuppliers()
        {
            // Arrange
            var controller = new SupplierController();

            // Act
            var result = controller.GetSuppliers();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var suppliers = Assert.IsAssignableFrom<IEnumerable<Supplier>>(actionResult.Value);
            suppliers.Count().Should().Be(2);
        }

        [Fact]
        public void GetSupplier_WithExistingId_ShouldReturnSupplier()
        {
            // Arrange
            var controller = new SupplierController();
            var expectedSupplierId = 1;

            // Act
            var result = controller.GetSupplier(expectedSupplierId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var supplier = Assert.IsType<Supplier>(actionResult.Value);
            supplier.Id.Should().Be(expectedSupplierId);
        }

        [Fact]
        public void GetSupplier_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SupplierController();
            var nonExistingId = 99;

            // Act
            var result = controller.GetSupplier(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void CreateSupplier_ShouldAddSupplierAndReturnCreatedAtAction()
        {
            // Arrange
            var controller = new SupplierController();
            var newSupplier = new Supplier { Name = "New Supplier", Email = "new@supplier.com" };

            // Act
            var result = controller.CreateSupplier(newSupplier);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdSupplier = Assert.IsType<Supplier>(actionResult.Value);
            createdSupplier.Should().NotBeNull();
            createdSupplier.Id.Should().BeGreaterThan(0);
            createdSupplier.Name.Should().Be(newSupplier.Name);
            createdSupplier.Email.Should().Be(newSupplier.Email);
        }

        [Fact]
        public void UpdateSupplier_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new SupplierController();
            var supplierIdToUpdate = 1;
            var updatedSupplier = new Supplier { Id = supplierIdToUpdate, Name = "Updated Name", Email = "updated@email.com" };

            // Act
            var result = controller.UpdateSupplier(supplierIdToUpdate, updatedSupplier);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void UpdateSupplier_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SupplierController();
            var nonExistingId = 99;
            var updatedSupplier = new Supplier { Id = nonExistingId, Name = "Updated Name", Email = "updated@email.com" };

            // Act
            var result = controller.UpdateSupplier(nonExistingId, updatedSupplier);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteSupplier_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            var controller = new SupplierController();
            var supplierIdToDelete = 1; // Assuming supplier with Id 1 exists initially

            // Act
            var result = controller.DeleteSupplier(supplierIdToDelete);

            // Assert
            Assert.IsType<NoContentResult>(result);
            // Optionally, verify that the supplier is actually removed
            // var getResult = controller.GetSupplier(supplierIdToDelete);
            // Assert.IsType<NotFoundResult>(getResult.Result);
        }

        [Fact]
        public void DeleteSupplier_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new SupplierController();
            var nonExistingId = 99;

            // Act
            var result = controller.DeleteSupplier(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
