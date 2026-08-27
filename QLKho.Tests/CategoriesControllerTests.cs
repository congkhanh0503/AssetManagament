using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetCategories_ReturnsAllCategories_WithAssetCount()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetCategories_ReturnsAllCategories_WithAssetCount));
        var controller = new CategoriesController(context);

        // Act
        var result = await controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Equal(2, categories.Count());

        var laptopCat = categories.First(c => c.CategoryCode == "LAPTOP");
        Assert.Equal(2, laptopCat.AssetCount); // AST-LT-001 + AST-LT-002
    }

    [Fact]
    public async Task CreateCategory_ValidInput_SavesAndReturnsCategory()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateCategory_ValidInput_SavesAndReturnsCategory));
        var controller = new CategoriesController(context);

        var dto = new CreateCategoryDto
        {
            CategoryCode = "MOUSE",
            CategoryName = "Chuột máy tính",
            Description = "Chuột không dây và có dây"
        };

        // Act
        var result = await controller.CreateCategory(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<AssetCategory>(okResult.Value);
        Assert.Equal("MOUSE", category.CategoryCode); // Must be uppercase
        Assert.Equal("Chuột máy tính", category.CategoryName);

        var dbCat = await context.AssetCategories.FirstOrDefaultAsync(c => c.CategoryCode == "MOUSE");
        Assert.NotNull(dbCat);
    }

    [Fact]
    public async Task CreateCategory_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateCategory_DuplicateCode_ReturnsBadRequest));
        var controller = new CategoriesController(context);

        var dto = new CreateCategoryDto
        {
            CategoryCode = "LAPTOP", // Already exists in seed
            CategoryName = "Duplicate Laptop Category"
        };

        // Act
        var result = await controller.CreateCategory(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("đã tồn tại", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task UpdateCategory_ValidData_UpdatesCorrectly()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateCategory_ValidData_UpdatesCorrectly));
        var controller = new CategoriesController(context);

        var dto = new CreateCategoryDto
        {
            CategoryCode = "LAPTOP-UPDATED",
            CategoryName = "Laptop và Máy tính xách tay (Cập nhật)",
            Description = "Bao gồm cả laptop gaming và office"
        };

        // Act
        var result = await controller.UpdateCategory(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.AssetCategories.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("LAPTOP-UPDATED", updated.CategoryCode);
        Assert.Equal("Laptop và Máy tính xách tay (Cập nhật)", updated.CategoryName);
    }

    [Fact]
    public async Task UpdateCategory_DuplicateCodeOnDifferentCategory_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateCategory_DuplicateCodeOnDifferentCategory_ReturnsBadRequest));
        var controller = new CategoriesController(context);

        var dto = new CreateCategoryDto
        {
            CategoryCode = "MONITOR", // Belongs to CategoryID=2
            CategoryName = "Trying to steal MONITOR code"
        };

        // Act
        var result = await controller.UpdateCategory(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCategory_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateCategory_NotFound_Returns404));
        var controller = new CategoriesController(context);

        var dto = new CreateCategoryDto
        {
            CategoryCode = "NONEXIST",
            CategoryName = "Does not exist"
        };

        // Act
        var result = await controller.UpdateCategory(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_WithAssets_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteCategory_WithAssets_ReturnsBadRequest));
        var controller = new CategoriesController(context);

        // CategoryID=1 (LAPTOP) has assets
        // Act
        var result = await controller.DeleteCategory(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_WithoutAssets_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteCategory_WithoutAssets_Succeeds));
        var controller = new CategoriesController(context);

        // Add a category without assets
        context.AssetCategories.Add(new AssetCategory
        {
            CategoryID = 100,
            CategoryCode = "EMPTY",
            CategoryName = "Empty Category"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteCategory(100);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.AssetCategories.AnyAsync(c => c.CategoryID == 100));
    }

    [Fact]
    public async Task DeleteCategory_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteCategory_NotFound_Returns404));
        var controller = new CategoriesController(context);

        // Act
        var result = await controller.DeleteCategory(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
