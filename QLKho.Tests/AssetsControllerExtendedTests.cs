using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

/// <summary>
/// Additional tests for AssetsController endpoints not covered in AssetsControllerTests.cs:
/// - GetAssetDetail
/// - TransferAsset
/// - DeleteAsset
/// - GetWarrantyAlerts
/// - Edge cases for Assign/Return
/// </summary>
public class AssetsControllerExtendedTests
{
    // ==================== GetAssetDetail ====================

    [Fact]
    public async Task GetAssetDetail_ValidId_ReturnsFullDetailWithHistoriesAndMaintenances()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssetDetail_ValidId_ReturnsFullDetailWithHistoriesAndMaintenances));
        var controller = new AssetsController(context);

        // Act
        var actionResult = await controller.GetAssetDetail(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(okResult.Value);

        // Verify it's an anonymous object with expected properties
        var value = okResult.Value!;
        Assert.Equal(1, (int)value.GetType().GetProperty("AssetID")!.GetValue(value)!);
        Assert.Equal("AST-LT-001", (string)value.GetType().GetProperty("AssetCode")!.GetValue(value)!);
        Assert.Equal("Dell XPS 13", (string)value.GetType().GetProperty("AssetName")!.GetValue(value)!);
    }

    [Fact]
    public async Task GetAssetDetail_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssetDetail_NotFound_Returns404));
        var controller = new AssetsController(context);

        // Act
        var actionResult = await controller.GetAssetDetail(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // ==================== TransferAsset ====================

    [Fact]
    public async Task TransferAsset_ValidInput_ChangesHolderAndCreatesHistory()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(TransferAsset_ValidInput_ChangesHolderAndCreatesHistory));
        var controller = new AssetsController(context);

        // Asset 2 is In-Use, held by Employee 1. Transfer to Employee 2.
        var dto = new TransferAssetDto
        {
            AssetID = 2,
            ToEmployeeID = 2,
            ConditionStatus = "Bình thường",
            Note = "Điều chuyển chéo bộ phận"
        };

        // Act
        var result = await controller.TransferAsset(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var asset = await context.Assets.FindAsync(2);
        Assert.NotNull(asset);
        Assert.Equal(2, asset.CurrentHolderID); // New holder
        Assert.Equal("In-Use", asset.Status);

        var history = await context.AssetHandoverHistories
            .FirstOrDefaultAsync(h => h.AssetID == 2 && h.ActionType == "Transfer");
        Assert.NotNull(history);
        Assert.Equal(1, history.FromEmployeeID); // Old holder
        Assert.Equal(2, history.ToEmployeeID);   // New holder
    }

    [Fact]
    public async Task TransferAsset_ToSameEmployee_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(TransferAsset_ToSameEmployee_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Asset 2 is held by Employee 1, try transferring to Employee 1
        var dto = new TransferAssetDto
        {
            AssetID = 2,
            ToEmployeeID = 1 // Same as current holder
        };

        // Act
        var result = await controller.TransferAsset(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task TransferAsset_AssetNotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(TransferAsset_AssetNotFound_Returns404));
        var controller = new AssetsController(context);

        var dto = new TransferAssetDto
        {
            AssetID = 999,
            ToEmployeeID = 1
        };

        // Act
        var result = await controller.TransferAsset(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TransferAsset_TargetEmployeeNotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(TransferAsset_TargetEmployeeNotFound_Returns404));
        var controller = new AssetsController(context);

        var dto = new TransferAssetDto
        {
            AssetID = 2,
            ToEmployeeID = 999 // Non-existent employee
        };

        // Act
        var result = await controller.TransferAsset(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==================== AssignAsset Edge Cases ====================

    [Fact]
    public async Task AssignAsset_AlreadyInUse_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(AssignAsset_AlreadyInUse_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Asset 2 is In-Use
        var dto = new AssignAssetDto
        {
            AssetID = 2,
            ToEmployeeID = 2
        };

        // Act
        var result = await controller.AssignAsset(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssignAsset_BrokenAsset_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(AssignAsset_BrokenAsset_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Asset 3 is Broken
        var dto = new AssignAssetDto
        {
            AssetID = 3,
            ToEmployeeID = 1
        };

        // Act
        var result = await controller.AssignAsset(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AssignAsset_AssetNotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(AssignAsset_AssetNotFound_Returns404));
        var controller = new AssetsController(context);

        var dto = new AssignAssetDto
        {
            AssetID = 999,
            ToEmployeeID = 1
        };

        // Act
        var result = await controller.AssignAsset(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==================== ReturnAsset Edge Cases ====================

    [Fact]
    public async Task ReturnAsset_AsBroken_ChangesStatusToBrokenAndCreatesMaintenance()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ReturnAsset_AsBroken_ChangesStatusToBrokenAndCreatesMaintenance));
        var controller = new AssetsController(context);

        var dto = new ReturnAssetDto
        {
            AssetID = 2, // In-Use, held by Employee 1
            IsBroken = true,
            ConditionStatus = "Màn hình vỡ, hỏng HDD",
            Note = "Trả máy hỏng"
        };

        // Act
        var result = await controller.ReturnAsset(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var asset = await context.Assets.FindAsync(2);
        Assert.NotNull(asset);
        Assert.Equal("Broken", asset.Status);
        Assert.Null(asset.CurrentHolderID);

        // Should also create a maintenance record
        var maintenance = await context.AssetMaintenances.FirstOrDefaultAsync(m => m.AssetID == 2);
        Assert.NotNull(maintenance);
        Assert.Equal("In-Progress", maintenance.Status);
    }

    [Fact]
    public async Task ReturnAsset_AssetNotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ReturnAsset_AssetNotFound_Returns404));
        var controller = new AssetsController(context);

        var dto = new ReturnAssetDto { AssetID = 999 };

        // Act
        var result = await controller.ReturnAsset(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==================== CreateAsset Edge Cases ====================

    [Fact]
    public async Task CreateAsset_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateAsset_DuplicateCode_ReturnsBadRequest));
        var controller = new AssetsController(context);

        var dto = new CreateAssetDto
        {
            AssetCode = "AST-LT-001", // Already exists
            AssetName = "Duplicate",
            CategoryID = 1
        };

        // Act
        var result = await controller.CreateAsset(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== UpdateAsset Edge Cases ====================

    [Fact]
    public async Task UpdateAsset_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateAsset_NotFound_Returns404));
        var controller = new AssetsController(context);

        var dto = new UpdateAssetDto
        {
            AssetCode = "AST-NOEXIST",
            AssetName = "Non-existent",
            CategoryID = 1
        };

        // Act
        var result = await controller.UpdateAsset(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateAsset_DuplicateCodeOnDifferentAsset_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateAsset_DuplicateCodeOnDifferentAsset_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Try to update Asset 2's code to AST-LT-001 (which belongs to Asset 1)
        var dto = new UpdateAssetDto
        {
            AssetCode = "AST-LT-001",
            AssetName = "Steal code",
            CategoryID = 1
        };

        // Act
        var result = await controller.UpdateAsset(2, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== DeleteAsset ====================

    [Fact]
    public async Task DeleteAsset_AvailableAsset_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteAsset_AvailableAsset_Succeeds));
        var controller = new AssetsController(context);

        // Asset 1 is Available
        // Act
        var result = await controller.DeleteAsset(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.Assets.AnyAsync(a => a.AssetID == 1));
    }

    [Fact]
    public async Task DeleteAsset_InUseAsset_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteAsset_InUseAsset_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Asset 2 is In-Use
        // Act
        var result = await controller.DeleteAsset(2);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAsset_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteAsset_NotFound_Returns404));
        var controller = new AssetsController(context);

        // Act
        var result = await controller.DeleteAsset(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==================== GetWarrantyAlerts ====================

    [Fact]
    public async Task GetWarrantyAlerts_ReturnsAssetsNearOrPastExpiry()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetWarrantyAlerts_ReturnsAssetsNearOrPastExpiry));
        var controller = new AssetsController(context);

        // Add assets with various warranty dates
        context.Assets.AddRange(
            new Asset
            {
                AssetCode = "AST-WAR-EXPIRED",
                AssetName = "Expired Warranty Laptop",
                CategoryID = 1,
                WarrantyExpireDate = DateTime.UtcNow.AddDays(-10), // Expired
                Status = "Available"
            },
            new Asset
            {
                AssetCode = "AST-WAR-NEAR",
                AssetName = "Near Expiry Laptop",
                CategoryID = 1,
                WarrantyExpireDate = DateTime.UtcNow.AddDays(15), // Within 30 days
                Status = "In-Use"
            },
            new Asset
            {
                AssetCode = "AST-WAR-FAR",
                AssetName = "Far Expiry Laptop",
                CategoryID = 1,
                WarrantyExpireDate = DateTime.UtcNow.AddDays(200), // Far future
                Status = "Available"
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetWarrantyAlerts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var list = (IEnumerable<dynamic>)okResult.Value!;
        var count = list.Count();
        Assert.True(count >= 2, "Should return at least 2 assets (expired + near expiry)");
    }

    // ==================== GetAssets Filtering ====================

    [Fact]
       public async Task GetAssets_FilterByStatus_ReturnsOnlyMatching()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_FilterByStatus_ReturnsOnlyMatching));
        var controller = new AssetsController(context);

        // Act
        var result = await controller.GetAssets(new AssetFilterDto { Status = "Available" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value);
        Assert.All(assets, a => Assert.Equal("Available", a.Status));
    }

    [Fact]
    public async Task GetAssets_FilterByCategory_ReturnsOnlyMatching()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_FilterByCategory_ReturnsOnlyMatching));
        var controller = new AssetsController(context);

        // Act: CategoryID=2 is MONITOR, only 1 asset
        var result = await controller.GetAssets(new AssetFilterDto { CategoryID = 2 });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value);
        Assert.Single(assets);
        Assert.Equal("AST-MN-001", assets.First().AssetCode);
    }

    [Fact]
    public async Task GetAssets_FilterByEmployee_ReturnsOnlyAssetsHeldByEmployee()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_FilterByEmployee_ReturnsOnlyAssetsHeldByEmployee));
        var controller = new AssetsController(context);

        // Employee 1 holds Asset 2 (In-Use)
        // Act
        var result = await controller.GetAssets(new AssetFilterDto { EmployeeID = 1 });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value);
        Assert.Single(assets);
        Assert.Equal("AST-LT-002", assets.First().AssetCode);
    }

    [Fact]
    public async Task GetAssets_SearchByName_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_SearchByName_ReturnsMatchingResults));
        var controller = new AssetsController(context);

        // Act
        var result = await controller.GetAssets(new AssetFilterDto { Search = "Dell" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value);
        Assert.All(assets, a => Assert.Contains("Dell", a.AssetName, StringComparison.OrdinalIgnoreCase));
    }

    // ==================== ImportBulk Edge Cases ====================

    [Fact]
    public async Task ImportBulkAssets_WithDuplicateCodesInSameBatch_SkipsDuplicates()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkAssets_WithDuplicateCodesInSameBatch_SkipsDuplicates));
        var controller = new AssetsController(context);

        var items = new List<ImportAssetItemDto>
        {
            new() { AssetCode = "AST-DUP-001", AssetName = "First", CategoryName = "Máy tính xách tay" },
            new() { AssetCode = "AST-DUP-001", AssetName = "Duplicate in same batch", CategoryName = "Máy tính xách tay" }
        };

        // Act
        var result = await controller.ImportBulkAssets(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);
        Assert.Equal(1, bulkRes.SuccessCount);
        Assert.Equal(1, bulkRes.SkippedCount);
    }

    [Fact]
    public async Task ImportBulkAssets_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkAssets_EmptyList_ReturnsBadRequest));
        var controller = new AssetsController(context);

        // Act
        var result = await controller.ImportBulkAssets(new List<ImportAssetItemDto>());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ImportBulkAssets_RowsMissingRequiredFields_AreSkipped()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkAssets_RowsMissingRequiredFields_AreSkipped));
        var controller = new AssetsController(context);

        var items = new List<ImportAssetItemDto>
        {
            new() { AssetCode = "", AssetName = "Missing code" }, // Missing AssetCode
            new() { AssetCode = "AST-VALID", AssetName = "" }, // Missing AssetName
            new() { AssetCode = "AST-GOOD", AssetName = "Valid Asset", CategoryName = "Máy tính xách tay" }
        };

        // Act
        var result = await controller.ImportBulkAssets(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);
        Assert.Equal(1, bulkRes.SuccessCount);
        Assert.Equal(2, bulkRes.SkippedCount);
    }

    // ==================== HandoverHistory Periods ====================

    [Fact]
    public async Task GetHandoverHistory_PeriodAll_ReturnsAllHistory()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetHandoverHistory_PeriodAll_ReturnsAllHistory));
        var controller = new AssetsController(context);

        // Add history with old date (beyond 30 days)
        context.AssetHandoverHistories.Add(new AssetHandoverHistory
        {
            AssetID = 1,
            ActionType = "Assign",
            ToEmployeeID = 1,
            ActionDate = DateTime.UtcNow.AddDays(-365), // Over a year ago
            Note = "Very old assignment"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetHandoverHistory(new HandoverHistoryFilterDto { Period = "all" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<HandoverHistoryItemDto>>(okResult.Value).ToList();
        Assert.Single(list); // Only the old one, since seed data has no histories
    }

    [Fact]
    public async Task GetHandoverHistory_PeriodLastMonth_ReturnsOnlyLastMonthRecords()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetHandoverHistory_PeriodLastMonth_ReturnsOnlyLastMonthRecords));
        var controller = new AssetsController(context);

        var now = DateTime.UtcNow;
        var firstOfCurrentMonth = new DateTime(now.Year, now.Month, 1);

        context.AssetHandoverHistories.AddRange(
            new AssetHandoverHistory
            {
                AssetID = 1,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ActionDate = firstOfCurrentMonth.AddDays(-15), // Mid last month
                Note = "Last month assignment"
            },
            new AssetHandoverHistory
            {
                AssetID = 2,
                ActionType = "Return",
                FromEmployeeID = 1,
                ActionDate = firstOfCurrentMonth.AddDays(10), // This month (should not appear)
                Note = "This month return"
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetHandoverHistory(new HandoverHistoryFilterDto { Period = "last_month" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<HandoverHistoryItemDto>>(okResult.Value).ToList();
        Assert.Single(list);
        Assert.Equal("Last month assignment", list[0].Note);
    }
}
