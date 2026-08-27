using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

public class AssetsControllerTests
{
    [Fact]
    public async Task GetAssets_ReturnsAllAssets_WithDynamicLocation()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_ReturnsAllAssets_WithDynamicLocation));
        var controller = new AssetsController(context);

        // Act
        var result = await controller.GetAssets(new AssetFilterDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value);
        Assert.Equal(3, assets.Count());

        // Kiểm tra logic Dynamic Location
        var inUseAsset = assets.First(a => a.AssetCode == "AST-LT-002");
        Assert.Equal("In-Use", inUseAsset.Status);
        Assert.Equal("Phòng Công Nghệ Thông Tin", inUseAsset.DynamicLocation); // Vị trí theo phòng ban của người giữ

        var availableAsset = assets.First(a => a.AssetCode == "AST-LT-001");
        Assert.Equal("Available", availableAsset.Status);
        Assert.Equal("Kho IT - Kệ A1", availableAsset.DynamicLocation); // Vị trí theo kho
    }

    [Fact]
    public async Task AssignAsset_ValidInput_ChangesStatusToInUse_AndCreatesHistory()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(AssignAsset_ValidInput_ChangesStatusToInUse_AndCreatesHistory));
        var controller = new AssetsController(context);

        var assignDto = new AssignAssetDto
        {
            AssetID = 1,
            ToEmployeeID = 1,
            ConditionStatus = "Mới 100%",
            Note = "Cấp máy làm việc"
        };

        // Act
        var result = await controller.AssignAsset(assignDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var asset = await context.Assets.FindAsync(1);
        Assert.NotNull(asset);
        Assert.Equal("In-Use", asset.Status);
        Assert.Equal(1, asset.CurrentHolderID);

        // Kiểm tra lịch sử
        var history = context.AssetHandoverHistories.FirstOrDefault(h => h.AssetID == 1 && h.ActionType == "Assign");
        Assert.NotNull(history);
        Assert.Equal(1, history.ToEmployeeID);
    }

    [Fact]
    public async Task ReturnAsset_ValidInput_ChangesStatusToAvailableOrBroken()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ReturnAsset_ValidInput_ChangesStatusToAvailableOrBroken));
        var controller = new AssetsController(context);

        var returnDto = new ReturnAssetDto
        {
            AssetID = 2,
            WarehouseLocation = "Kho IT - Kệ Thu Hồi",
            IsBroken = false,
            ConditionStatus = "Máy hoạt động bình thường",
            Note = "Nhân viên trả máy"
        };

        // Act
        var result = await controller.ReturnAsset(returnDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var asset = await context.Assets.FindAsync(2);
        Assert.NotNull(asset);
        Assert.Equal("Available", asset.Status);
        Assert.Null(asset.CurrentHolderID);
        Assert.Equal("Kho IT - Kệ Thu Hồi", asset.WarehouseLocation);
    }

    [Fact]
    public async Task ImportBulkAssets_ValidData_InsertsNewAssets()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkAssets_ValidData_InsertsNewAssets));
        var controller = new AssetsController(context);

        var items = new List<ImportAssetItemDto>
        {
            new ImportAssetItemDto
            {
                AssetCode = "AST-IMP-001",
                AssetName = "Lenovo ThinkPad X1 Carbon",
                CategoryName = "Máy tính xách tay",
                Brand = "Lenovo",
                WarehouseLocation = "Kho IT - Kệ A1"
            },
            new ImportAssetItemDto
            {
                AssetCode = "AST-IMP-002",
                AssetName = "Màn hình LG 27-inch 4K",
                CategoryName = "Màn hình",
                Brand = "LG",
                WarehouseLocation = "Kho IT - Kệ A2"
            }
        };

        // Act
        var result = await controller.ImportBulkAssets(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);

        Assert.Equal(2, bulkRes.SuccessCount);
        Assert.Equal(0, bulkRes.SkippedCount);
        Assert.True(context.Assets.Any(a => a.AssetCode == "AST-IMP-001"));
    }

    [Fact]
    public async Task GetHandoverHistory_FiltersByPeriodAndActionType_ReturnsCorrectList()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetHandoverHistory_FiltersByPeriodAndActionType_ReturnsCorrectList));
        var controller = new AssetsController(context);

        // Tạo sẵn một số lịch sử
        context.AssetHandoverHistories.AddRange(
            new AssetHandoverHistory
            {
                AssetID = 1,
                ActionType = "Assign",
                ToEmployeeID = 1,
                ActionDate = DateTime.UtcNow.AddDays(-2),
                Note = "Cấp mới máy cho NV"
            },
            new AssetHandoverHistory
            {
                AssetID = 2,
                ActionType = "Return",
                FromEmployeeID = 2,
                ActionDate = DateTime.UtcNow.AddDays(-5),
                Note = "Thu hồi máy"
            }
        );
        await context.SaveChangesAsync();

        // Act: Lọc theo tuần và Assign
        var filter = new HandoverHistoryFilterDto
        {
            Period = "week",
            ActionType = "Assign"
        };
        var result = await controller.GetHandoverHistory(filter);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<HandoverHistoryItemDto>>(okResult.Value).ToList();

        Assert.Single(list);
        Assert.Equal("Assign", list[0].ActionType);
        Assert.Equal("Cấp Phát", list[0].ActionTypeLabel);
    }

    [Fact]
    public async Task GetAssets_OrdersByStatusPriority_CorrectSequence()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetAssets_OrdersByStatusPriority_CorrectSequence));
        var controller = new AssetsController(context);

        // Thêm các máy có trạng thái khác nhau
        context.Assets.AddRange(
            new Asset { AssetCode = "AST-BROKEN", AssetName = "Broken Laptop", CategoryID = 1, Status = "Broken" },
            new Asset { AssetCode = "AST-MAINT", AssetName = "Maintenance Screen", CategoryID = 2, Status = "Maintenance" },
            new Asset { AssetCode = "AST-AVAIL", AssetName = "Available PC", CategoryID = 1, Status = "Available" },
            new Asset { AssetCode = "AST-INUSE", AssetName = "InUse Laptop", CategoryID = 1, Status = "In-Use" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAssets(new AssetFilterDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value).ToList();

        // Kiểm tra thứ tự: In-Use (1) -> Available (2) -> Maintenance (3) -> Broken (4)
        var statuses = list.Select(a => a.Status).ToList();
        var indexInUse = statuses.IndexOf("In-Use");
        var indexAvailable = statuses.IndexOf("Available");
        var indexMaintenance = statuses.IndexOf("Maintenance");
        var indexBroken = statuses.IndexOf("Broken");

        Assert.True(indexInUse < indexAvailable, "In-Use phải đứng trước Available");
        Assert.True(indexAvailable < indexMaintenance, "Available phải đứng trước Maintenance");
        Assert.True(indexMaintenance < indexBroken, "Maintenance phải đứng trước Broken");
    }

    [Fact]
    public async Task CreateAsset_WithSpecifications_SavesAndReturnsSpecifications()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateAsset_WithSpecifications_SavesAndReturnsSpecifications));
        var controller = new AssetsController(context);

        var dto = new CreateAssetDto
        {
            AssetCode = "AST-SPEC-001",
            AssetName = "Dell XPS 15 9530",
            CategoryID = 1,
            Brand = "Dell",
            Specifications = "Intel Core i9-13900H, 32GB RAM, 1TB SSD, RTX 4070",
            Status = "Available"
        };

        // Act
        var result = await controller.CreateAsset(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var asset = Assert.IsType<Asset>(createdResult.Value);

        Assert.Equal("Intel Core i9-13900H, 32GB RAM, 1TB SSD, RTX 4070", asset.Specifications);

        // Kiểm tra qua GetAssets
        var getResult = await controller.GetAssets(new AssetFilterDto { Search = "AST-SPEC-001" });
        var okResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<AssetItemDto>>(okResult.Value).ToList();

        Assert.Single(list);
        Assert.Equal("Intel Core i9-13900H, 32GB RAM, 1TB SSD, RTX 4070", list[0].Specifications);
    }

    [Fact]
    public async Task UpdateAsset_ValidData_UpdatesAllFieldsCorrectly()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateAsset_ValidData_UpdatesAllFieldsCorrectly));
        var controller = new AssetsController(context);

        var asset = new Asset
        {
            AssetCode = "AST-UPD-001",
            AssetName = "Old Laptop",
            CategoryID = 1,
            Brand = "HP",
            Status = "Available",
            WarehouseLocation = "Kho IT - Kệ A1"
        };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var updateDto = new UpdateAssetDto
        {
            AssetCode = "AST-UPD-001",
            AssetName = "HP EliteBook 840 G10",
            CategoryID = 1,
            Brand = "HP",
            Specifications = "Core i7-1360P, 16GB RAM, 512GB SSD",
            Status = "Available",
            WarehouseLocation = "Kho IT - Kệ VIP"
        };

        // Act
        var result = await controller.UpdateAsset(asset.AssetID, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Assets.FindAsync(asset.AssetID);
        Assert.NotNull(updated);
        Assert.Equal("HP EliteBook 840 G10", updated.AssetName);
        Assert.Equal("Core i7-1360P, 16GB RAM, 512GB SSD", updated.Specifications);
        Assert.Equal("Kho IT - Kệ VIP", updated.WarehouseLocation);
    }

    [Fact]
    public async Task UpdateAsset_InUseAsset_PreservesInUseStatusAndHolder()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateAsset_InUseAsset_PreservesInUseStatusAndHolder));
        var controller = new AssetsController(context);

        var asset = new Asset
        {
            AssetCode = "AST-INUSE-001",
            AssetName = "MacBook Pro M2",
            CategoryID = 1,
            Status = "In-Use",
            CurrentHolderID = 10,
            WarehouseLocation = "Bàn làm việc NV"
        };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Giả sử DTO gửi lên Status là Available (do form gửi nhầm hoặc không chọn)
        var updateDto = new UpdateAssetDto
        {
            AssetCode = "AST-INUSE-001",
            AssetName = "MacBook Pro M2 14-inch (Updated Name)",
            CategoryID = 1,
            Specifications = "M2 Pro 12-Core, 32GB RAM, 1TB SSD",
            Status = "Available" // Cố ý gửi Available
        };

        // Act
        var result = await controller.UpdateAsset(asset.AssetID, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Assets.FindAsync(asset.AssetID);
        Assert.NotNull(updated);
        Assert.Equal("MacBook Pro M2 14-inch (Updated Name)", updated.AssetName);
        Assert.Equal("M2 Pro 12-Core, 32GB RAM, 1TB SSD", updated.Specifications);
        Assert.Equal("In-Use", updated.Status); // Phải luôn giữ In-Use
        Assert.Equal(10, updated.CurrentHolderID); // Phải luôn giữ người đang cầm máy
    }

    [Fact]
    public async Task ReportIssue_ValidDto_CreatesMaintenanceAndHistorySuccessfully()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ReportIssue_ValidDto_CreatesMaintenanceAndHistorySuccessfully));
        var controller = new AssetsController(context);

        var asset = new Asset
        {
            AssetCode = "AST-ISSUE-001",
            AssetName = "Dell Monitor 27",
            CategoryID = 2,
            Status = "In-Use",
            CurrentHolderID = 5
        };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var dto = new ReportIssueDto
        {
            AssetID = asset.AssetID,
            IssueDescription = "Màn hình sọc ngang, chớp giật liên tục",
            Status = "Broken",
            VendorName = "Dell Support Vietnam",
            EstimatedCost = 500000,
            ExpectedReturnDate = DateTime.UtcNow.AddDays(7),
            Note = "Gửi hãng sửa bảo hành"
        };

        // Act
        var result = await controller.ReportIssue(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updatedAsset = await context.Assets.FindAsync(asset.AssetID);
        Assert.NotNull(updatedAsset);
        Assert.Equal("Broken", updatedAsset.Status);
        Assert.Null(updatedAsset.CurrentHolderID); // Đã tự động thu hồi khỏi nhân viên

        // Kiểm tra tạo bản ghi Maintenance
        var maintenance = await context.AssetMaintenances.FirstOrDefaultAsync(m => m.AssetID == asset.AssetID);
        Assert.NotNull(maintenance);
        Assert.Equal("In-Progress", maintenance.Status);
        Assert.Equal("Màn hình sọc ngang, chớp giật liên tục", maintenance.IssueDescription);
        Assert.Equal("Dell Support Vietnam", maintenance.VendorName);
        Assert.Equal(500000, maintenance.Cost);

        // Kiểm tra tạo bản ghi HandoverHistory
        var history = await context.AssetHandoverHistories.FirstOrDefaultAsync(h => h.AssetID == asset.AssetID);
        Assert.NotNull(history);
        Assert.Equal("SendMaintenance", history.ActionType);
    }
}
