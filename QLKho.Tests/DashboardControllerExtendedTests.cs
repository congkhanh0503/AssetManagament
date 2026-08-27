using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

/// <summary>
/// Additional tests for DashboardController endpoints not covered in DashboardControllerTests.cs:
/// - GetPeriodHighlights (week/month, top departments)
/// - GetBrokenHighlights
/// - GetCategoryDistribution
/// </summary>
public class DashboardControllerExtendedTests
{
    // ==================== GetPeriodHighlights ====================

    [Fact]
    public async Task GetPeriodHighlights_Week_ReturnsCorrectCounts()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetPeriodHighlights_Week_ReturnsCorrectCounts));
        var controller = new DashboardController(context);

        // Add history within last 7 days
        context.AssetHandoverHistories.AddRange(
            new AssetHandoverHistory
            {
                AssetID = 1,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ActionDate = DateTime.UtcNow.AddDays(-2),
                Note = "Cấp phát trong tuần"
            },
            new AssetHandoverHistory
            {
                AssetID = 2,
                ActionType = "Return",
                FromEmployeeID = 1,
                ActionDate = DateTime.UtcNow.AddDays(-3),
                Note = "Thu hồi trong tuần"
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPeriodHighlights("week");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var highlight = Assert.IsType<PeriodHighlightDto>(okResult.Value);
        Assert.Equal("week", highlight.PeriodType);
        Assert.Equal(1, highlight.AssignedCount);
        Assert.Equal(1, highlight.ReturnedCount);
    }

    [Fact]
    public async Task GetPeriodHighlights_Month_ReturnsCorrectCounts()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetPeriodHighlights_Month_ReturnsCorrectCounts));
        var controller = new DashboardController(context);

        context.AssetHandoverHistories.AddRange(
            new AssetHandoverHistory
            {
                AssetID = 1,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ActionDate = DateTime.UtcNow.AddDays(-15),
                Note = "Cấp phát trong tháng"
            },
            new AssetHandoverHistory
            {
                AssetID = 2,
                ActionType = "Return",
                FromEmployeeID = 1,
                ActionDate = DateTime.UtcNow.AddDays(-20),
                Note = "Thu hồi trong tháng"
            },
            new AssetHandoverHistory
            {
                AssetID = 3,
                ActionType = "Assign",
                ToEmployeeID = 2,
                ActionDate = DateTime.UtcNow.AddDays(-60), // Over a month ago, should not count
                Note = "Cấp phát ngoài tháng"
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPeriodHighlights("month");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var highlight = Assert.IsType<PeriodHighlightDto>(okResult.Value);
        Assert.Equal("month", highlight.PeriodType);
        Assert.Equal(1, highlight.AssignedCount); // Only the one within 30 days
        Assert.Equal(1, highlight.ReturnedCount);
    }

    [Fact]
    public async Task GetPeriodHighlights_TopAssignedDepartments_ReturnsSortedByCount()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetPeriodHighlights_TopAssignedDepartments_ReturnsSortedByCount));
        var controller = new DashboardController(context);

        // Add multiple assignments to different departments
        context.AssetHandoverHistories.AddRange(
            new AssetHandoverHistory
            {
                AssetID = 1,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ToDepartmentID = 1, // IT
                ActionDate = DateTime.UtcNow.AddDays(-5)
            },
            new AssetHandoverHistory
            {
                AssetID = 2,
                ActionType = "Assign",
                ToEmployeeID = 2,
                ToDepartmentID = 2, // HR
                ActionDate = DateTime.UtcNow.AddDays(-5)
            },
            new AssetHandoverHistory
            {
                AssetID = 3,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ToDepartmentID = 1, // IT again - should make IT top
                ActionDate = DateTime.UtcNow.AddDays(-3)
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPeriodHighlights("week");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var highlight = Assert.IsType<PeriodHighlightDto>(okResult.Value);
        Assert.NotEmpty(highlight.TopAssignedDepartments);

        // IT (DepartmentID=1) should be first with 2 assignments
        var topDept = highlight.TopAssignedDepartments.First();
        Assert.Equal(1, topDept.DepartmentID);
        Assert.Equal(2, topDept.AssetCount);
    }

    [Fact]
    public async Task GetPeriodHighlights_NoHistory_ReturnsZeroCounts()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetPeriodHighlights_NoHistory_ReturnsZeroCounts));
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetPeriodHighlights("week");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var highlight = Assert.IsType<PeriodHighlightDto>(okResult.Value);
        Assert.Equal(0, highlight.AssignedCount);
        Assert.Equal(0, highlight.ReturnedCount);
        Assert.Equal(0, highlight.BrokenReportedCount);
    }

    // ==================== GetBrokenHighlights ====================

    [Fact]
    public async Task GetBrokenHighlights_ReturnsBrokenAndMaintenanceAssets()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetBrokenHighlights_ReturnsBrokenAndMaintenanceAssets));
        var controller = new DashboardController(context);

        // Add a maintenance asset
        context.Assets.Add(new Asset
        {
            AssetCode = "AST-MAINT-001",
            AssetName = "Monitor under maintenance",
            CategoryID = 2,
            Status = "Maintenance",
            WarehouseLocation = "Trung tâm bảo hành"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetBrokenHighlights();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<BrokenAssetHighlightDto>>(okResult.Value).ToList();
        // Asset 3 (Broken) + AST-MAINT-001 (Maintenance) = 2
        Assert.Equal(2, list.Count);
        Assert.Contains(list, a => a.Status == "Broken");
        Assert.Contains(list, a => a.Status == "Maintenance");
    }

    [Fact]
    public async Task GetBrokenHighlights_NoBrokenOrMaintenance_ReturnsEmpty()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetBrokenHighlights_NoBrokenOrMaintenance_ReturnsEmpty));
        var controller = new DashboardController(context);

        // Remove the broken asset from seed
        var broken = await context.Assets.FirstOrDefaultAsync(a => a.Status == "Broken");
        if (broken != null)
        {
            context.Assets.Remove(broken);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await controller.GetBrokenHighlights();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<BrokenAssetHighlightDto>>(okResult.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetBrokenHighlights_ReturnsLatestMaintenanceIssue()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetBrokenHighlights_ReturnsLatestMaintenanceIssue));
        var controller = new DashboardController(context);

        // Add maintenance records for Asset 3 (Broken)
        context.AssetMaintenances.Add(new AssetMaintenance
        {
            AssetID = 3,
            IssueDescription = "Màn hình không hiển thị",
            SentDate = DateTime.UtcNow.AddDays(-3),
            Status = "In-Progress",
            VendorName = "Dell Support"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetBrokenHighlights();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<BrokenAssetHighlightDto>>(okResult.Value).ToList();
        var broken = Assert.Single(list);
        Assert.Equal("Màn hình không hiển thị", broken.LatestIssue);
        Assert.Equal("Dell Support", broken.VendorName);
    }

    // ==================== GetCategoryDistribution ====================

    [Fact]
    public async Task GetCategoryDistribution_ReturnsCategoriesWithCountAndPercentage()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetCategoryDistribution_ReturnsCategoriesWithCountAndPercentage));
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetCategoryDistribution();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CategoryDistributionDto>>(okResult.Value).ToList();
        Assert.Equal(2, list.Count);

        // LAPTOP has 2 assets, MONITOR has 1 -> total 3
        var laptop = list.First();
        Assert.Equal(2, laptop.AssetCount);
        Assert.Equal(66.7, Math.Round(laptop.Percentage, 1)); // 2/3 * 100 = 66.67
    }

    [Fact]
    public async Task GetCategoryDistribution_NoAssets_ReturnsAllCategoriesWithZero()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetCategoryDistribution_NoAssets_ReturnsAllCategoriesWithZero));
        var controller = new DashboardController(context);

        // Remove all assets
        context.Assets.RemoveRange(context.Assets);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetCategoryDistribution();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CategoryDistributionDto>>(okResult.Value).ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.Equal(0, c.AssetCount));
        Assert.All(list, c => Assert.Equal(0, c.Percentage));
    }

    [Fact]
    public async Task GetCategoryDistribution_SortedByAssetCountDescending()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetCategoryDistribution_SortedByAssetCountDescending));
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetCategoryDistribution();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CategoryDistributionDto>>(okResult.Value).ToList();
        // LAPTOP (2 assets) should be before MONITOR (1 asset)
        Assert.True(list[0].AssetCount >= list[1].AssetCount);
    }

    // ==================== GetKpis Additional Tests ====================

    [Fact]
    public async Task GetKpis_CalculatesResignedAlertsCorrectly()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetKpis_CalculatesResignedAlertsCorrectly));
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetKpis();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var kpi = Assert.IsType<DashboardKpiDto>(okResult.Value);
        // Employee 2 has LeaveDate and QAD_Status = "Available" (not disabled) -> should be 1 alert
        Assert.Equal(1, kpi.ResignedEmployeesWithAlerts);
    }

    [Fact]
    public async Task GetKpis_MaintenanceAndDisposedCounts()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetKpis_MaintenanceAndDisposedCounts));
        var controller = new DashboardController(context);

        // Add maintenance and disposed assets
        context.Assets.AddRange(
            new Asset { AssetCode = "AST-MAINT-KPI", AssetName = "Maintenance Asset", CategoryID = 1, Status = "Maintenance" },
            new Asset { AssetCode = "AST-DISP-KPI", AssetName = "Disposed Asset", CategoryID = 1, Status = "Disposed" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetKpis();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var kpi = Assert.IsType<DashboardKpiDto>(okResult.Value);
        Assert.Equal(1, kpi.MaintenanceAssets);
        Assert.Equal(1, kpi.DisposedAssets);
        Assert.Equal(5, kpi.TotalAssets); // 3 seed + 2 added
    }
}
