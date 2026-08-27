using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

/// <summary>
/// Additional tests for EmployeesController endpoints not covered in EmployeesControllerTests.cs:
/// - GetEmployeeDetail
/// - GetEmployeeAssets
/// - CreateEmployee (duplicate code, validation)
/// - UpdateEmployee (duplicate code, not found)
/// - DeleteEmployee (with/without assets)
/// - GetEmployees filtering (search, department, status)
/// </summary>
public class EmployeesControllerExtendedTests
{
    // ==================== GetEmployeeDetail ====================

    [Fact]
    public async Task GetEmployeeDetail_ValidId_ReturnsFullDetail()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployeeDetail_ValidId_ReturnsFullDetail));
        var controller = new EmployeesController(context);

        // Act
        var actionResult = await controller.GetEmployeeDetail(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(okResult.Value);

        var value = okResult.Value!;
        Assert.Equal(1, (int)value.GetType().GetProperty("EmployeeID")!.GetValue(value)!);
        Assert.Equal("EMP001", (string)value.GetType().GetProperty("EmployeeCode")!.GetValue(value)!);
        Assert.Equal("Nguyễn Văn A", (string)value.GetType().GetProperty("FullName")!.GetValue(value)!);
    }

    [Fact]
    public async Task GetEmployeeDetail_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployeeDetail_NotFound_Returns404));
        var controller = new EmployeesController(context);

        // Act
        var actionResult = await controller.GetEmployeeDetail(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // ==================== GetEmployeeAssets ====================

    [Fact]
    public async Task GetEmployeeAssets_ReturnsAllInUseAssetsForEmployee()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployeeAssets_ReturnsAllInUseAssetsForEmployee));
        var controller = new EmployeesController(context);

        // Employee 1 holds Asset 2 (In-Use)
        // Act
        var actionResult = await controller.GetEmployeeAssets(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(okResult.Value);

        var value = okResult.Value!;
        Assert.Equal(1, (int)value.GetType().GetProperty("EmployeeID")!.GetValue(value)!);
        Assert.Equal("Nguyễn Văn A", (string)value.GetType().GetProperty("FullName")!.GetValue(value)!);

        var assetsProp = value.GetType().GetProperty("Assets")!.GetValue(value)!;
        var assetsList = ((System.Collections.IEnumerable)assetsProp).Cast<object>().ToList();
        Assert.Single(assetsList);
        var firstAsset = assetsList[0];
        Assert.Equal("AST-LT-002", (string)firstAsset.GetType().GetProperty("AssetCode")!.GetValue(firstAsset)!);
    }

    [Fact]
    public async Task GetEmployeeAssets_EmployeeNotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployeeAssets_EmployeeNotFound_Returns404));
        var controller = new EmployeesController(context);

        // Act
        var actionResult = await controller.GetEmployeeAssets(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetEmployeeAssets_EmployeeWithNoAssets_ReturnsEmptyList()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployeeAssets_EmployeeWithNoAssets_ReturnsEmptyList));
        var controller = new EmployeesController(context);

        // Employee 2 doesn't hold any assets
        // Act
        var actionResult = await controller.GetEmployeeAssets(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var value = okResult.Value!;
        Assert.Equal(0, (int)value.GetType().GetProperty("TotalHeld")!.GetValue(value)!);
    }

    // ==================== CreateEmployee Edge Cases ====================

    [Fact]
    public async Task CreateEmployee_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateEmployee_DuplicateCode_ReturnsBadRequest));
        var controller = new EmployeesController(context);

        var dto = new CreateEmployeeDto
        {
            EmployeeCode = "EMP001", // Already exists
            FullName = "Duplicate Employee",
            DepartmentID = 1
        };

        // Act
        var result = await controller.CreateEmployee(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== UpdateEmployee Edge Cases ====================

    [Fact]
    public async Task UpdateEmployee_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateEmployee_NotFound_Returns404));
        var controller = new EmployeesController(context);

        var dto = new UpdateEmployeeDto
        {
            EmployeeCode = "EMP-NOEXIST",
            FullName = "Non-existent",
            DepartmentID = 1
        };

        // Act
        var result = await controller.UpdateEmployee(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEmployee_DuplicateCodeOnDifferentEmployee_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateEmployee_DuplicateCodeOnDifferentEmployee_ReturnsBadRequest));
        var controller = new EmployeesController(context);

        var dto = new UpdateEmployeeDto
        {
            EmployeeCode = "EMP001", // Belongs to Employee 1, try to set on Employee 2
            FullName = "Trying to steal code",
            DepartmentID = 1
        };

        // Act
        var result = await controller.UpdateEmployee(2, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== DeleteEmployee ====================

    [Fact]
    public async Task DeleteEmployee_WithInUseAssets_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteEmployee_WithInUseAssets_ReturnsBadRequest));
        var controller = new EmployeesController(context);

        // Employee 1 holds Asset 2 (In-Use)
        // Act
        var result = await controller.DeleteEmployee(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteEmployee_WithoutAssets_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteEmployee_WithoutAssets_Succeeds));
        var controller = new EmployeesController(context);

        // Employee 2 doesn't hold any In-Use assets
        // Act
        var result = await controller.DeleteEmployee(2);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.Employees.AnyAsync(e => e.EmployeeID == 2));
    }

    [Fact]
    public async Task DeleteEmployee_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteEmployee_NotFound_Returns404));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.DeleteEmployee(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==================== GetEmployees Filtering ====================

    [Fact]
    public async Task GetEmployees_FilterByDepartment_ReturnsOnlyMatching()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_FilterByDepartment_ReturnsOnlyMatching));
        var controller = new EmployeesController(context);

        // Act: Department 1 = IT, has Employee 1
        var result = await controller.GetEmployees(null, 1, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Single(employees);
        Assert.Equal("EMP001", employees.First().EmployeeCode);
    }

    [Fact]
    public async Task GetEmployees_FilterByStatus_ReturnsOnlyMatching()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_FilterByStatus_ReturnsOnlyMatching));
        var controller = new EmployeesController(context);

        // Act: Status = "Resigned" -> only Employee 2
        var result = await controller.GetEmployees(null, null, "Resigned");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Single(employees);
        Assert.Equal("Resigned", employees.First().Status);
    }

    [Fact]
    public async Task GetEmployees_SearchByName_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_SearchByName_ReturnsMatchingResults));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.GetEmployees("Văn A", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Single(employees);
        Assert.Contains("Văn A", employees.First().FullName);
    }

    [Fact]
    public async Task GetEmployees_SearchByCode_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_SearchByCode_ReturnsMatchingResults));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.GetEmployees("EMP002", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Single(employees);
        Assert.Equal("EMP002", employees.First().EmployeeCode);
    }

    [Fact]
    public async Task GetEmployees_SearchByEmail_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_SearchByEmail_ReturnsMatchingResults));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.GetEmployees("a.nguyen", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Single(employees);
        Assert.Contains("a.nguyen", employees.First().Email!, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== ImportBulk Edge Cases ====================

    [Fact]
    public async Task ImportBulkEmployees_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkEmployees_EmptyList_ReturnsBadRequest));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.ImportBulkEmployees(new List<ImportEmployeeItemDto>());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ImportBulkEmployees_WithDuplicatesInBatch_SkipsDuplicates()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkEmployees_WithDuplicatesInBatch_SkipsDuplicates));
        var controller = new EmployeesController(context);

        var items = new List<ImportEmployeeItemDto>
        {
            new() { EmployeeCode = "EMP-DUP-001", FullName = "First", DepartmentName = "Phòng Công Nghệ Thông Tin" },
            new() { EmployeeCode = "EMP-DUP-001", FullName = "Duplicate in same batch", DepartmentName = "Phòng Công Nghệ Thông Tin" }
        };

        // Act
        var result = await controller.ImportBulkEmployees(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);
        Assert.Equal(1, bulkRes.SuccessCount);
        Assert.Equal(1, bulkRes.SkippedCount);
    }

    [Fact]
    public async Task ImportBulkEmployees_RowsMissingRequiredFields_AreSkipped()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkEmployees_RowsMissingRequiredFields_AreSkipped));
        var controller = new EmployeesController(context);

        var items = new List<ImportEmployeeItemDto>
        {
            new() { EmployeeCode = "", FullName = "Missing code" }, // Missing EmployeeCode
            new() { EmployeeCode = "EMP-VALID", FullName = "" }, // Missing FullName
            new() { EmployeeCode = "EMP-GOOD", FullName = "Valid Employee", DepartmentName = "Phòng Công Nghệ Thông Tin" }
        };

        // Act
        var result = await controller.ImportBulkEmployees(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);
        Assert.Equal(1, bulkRes.SuccessCount);
        Assert.Equal(2, bulkRes.SkippedCount);
    }

    // ==================== GetLeaveAlerts Edge Cases ====================

    [Fact]
    public async Task GetLeaveAlerts_EmployeeFullyDisabled_NoAlert()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetLeaveAlerts_EmployeeFullyDisabled_NoAlert));
        var controller = new EmployeesController(context);

        // Modify Employee 2 to have all accounts disabled (should NOT appear in alerts)
        var emp2 = await context.Employees.FindAsync(2);
        Assert.NotNull(emp2);
        emp2!.QAD_Status = "Disable"; // Fix the one that was "Available"
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetLeaveAlerts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var alerts = Assert.IsAssignableFrom<IEnumerable<EmployeeAlertDto>>(okResult.Value);
        Assert.DoesNotContain(alerts, a => a.EmployeeCode == "EMP002");
    }
}
