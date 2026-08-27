using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

public class SuppliersControllerTests
{
    [Fact]
    public async Task GetSuppliers_ReturnsAllSuppliers_WithAssetCount()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetSuppliers_ReturnsAllSuppliers_WithAssetCount));
        var controller = new SuppliersController(context);

        // Act
        var result = await controller.GetSuppliers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var suppliers = Assert.IsAssignableFrom<IEnumerable<SupplierDto>>(okResult.Value);
        Assert.Single(suppliers); // Only SUP-FPT

        var fpt = suppliers.First();
        Assert.Equal("FPT Services", fpt.SupplierName);
        Assert.Equal(3, fpt.SuppliedAssetCount); // 3 assets seeded
    }

    [Fact]
    public async Task CreateSupplier_ValidInput_SavesAndReturnsSupplier()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateSupplier_ValidInput_SavesAndReturnsSupplier));
        var controller = new SuppliersController(context);

        var dto = new CreateSupplierDto
        {
            SupplierCode = "SUP-HP",
            SupplierName = "HP Vietnam",
            ContactPerson = "Phạm Minh",
            Phone = "0901234567",
            Email = "contact@hp.com",
            Address = "Quận 1, TP.HCM"
        };

        // Act
        var result = await controller.CreateSupplier(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var supplier = Assert.IsType<Supplier>(okResult.Value);
        Assert.Equal("SUP-HP", supplier.SupplierCode); // Must be uppercase
        Assert.Equal("HP Vietnam", supplier.SupplierName);

        var dbSup = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == "SUP-HP");
        Assert.NotNull(dbSup);
    }

    [Fact]
    public async Task CreateSupplier_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateSupplier_DuplicateCode_ReturnsBadRequest));
        var controller = new SuppliersController(context);

        var dto = new CreateSupplierDto
        {
            SupplierCode = "SUP-FPT", // Already exists
            SupplierName = "Duplicate FPT"
        };

        // Act
        var result = await controller.CreateSupplier(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateSupplier_ValidData_UpdatesCorrectly()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateSupplier_ValidData_UpdatesCorrectly));
        var controller = new SuppliersController(context);

        var dto = new CreateSupplierDto
        {
            SupplierCode = "SUP-FPT-UPDATED",
            SupplierName = "FPT Services International",
            ContactPerson = "New Contact Person",
            Phone = "02873000000",
            Email = "new@fpt.com",
            Address = "New Address"
        };

        // Act
        var result = await controller.UpdateSupplier(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Suppliers.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("SUP-FPT-UPDATED", updated.SupplierCode);
        Assert.Equal("FPT Services International", updated.SupplierName);
        Assert.Equal("New Contact Person", updated.ContactPerson);
    }

    [Fact]
    public async Task UpdateSupplier_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateSupplier_NotFound_Returns404));
        var controller = new SuppliersController(context);

        var dto = new CreateSupplierDto
        {
            SupplierCode = "SUP-NOEXIST",
            SupplierName = "Non-existent"
        };

        // Act
        var result = await controller.UpdateSupplier(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSupplier_DuplicateCodeOnDifferentSupplier_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateSupplier_DuplicateCodeOnDifferentSupplier_ReturnsBadRequest));
        var controller = new SuppliersController(context);

        // Add a second supplier
        context.Suppliers.Add(new Supplier
        {
            SupplierID = 2,
            SupplierCode = "SUP-DELL",
            SupplierName = "Dell Vietnam"
        });
        await context.SaveChangesAsync();

        var dto = new CreateSupplierDto
        {
            SupplierCode = "SUP-DELL", // Trying to set supplier 1's code to SUP-DELL
            SupplierName = "Steal code"
        };

        // Act
        var result = await controller.UpdateSupplier(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSupplier_WithLinkedAssets_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteSupplier_WithLinkedAssets_ReturnsBadRequest));
        var controller = new SuppliersController(context);

        // SupplierID=1 has 3 assets linked
        // Act
        var result = await controller.DeleteSupplier(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSupplier_WithoutAssets_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteSupplier_WithoutAssets_Succeeds));
        var controller = new SuppliersController(context);

        context.Suppliers.Add(new Supplier
        {
            SupplierID = 200,
            SupplierCode = "SUP-EMPTY",
            SupplierName = "Empty Supplier"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteSupplier(200);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.Suppliers.AnyAsync(s => s.SupplierID == 200));
    }

    [Fact]
    public async Task DeleteSupplier_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteSupplier_NotFound_Returns404));
        var controller = new SuppliersController(context);

        // Act
        var result = await controller.DeleteSupplier(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
