using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

/// <summary>
/// Additional tests for DepartmentsController endpoints not covered in DepartmentsControllerTests.cs:
/// - GetDepartments (verify counts)
/// - UpdateDepartment
/// - DeleteDepartment edge cases
/// </summary>
public class DepartmentsControllerExtendedTests
{
    [Fact]
    public async Task GetDepartments_ReturnsAllWithEmployeeAndAssetCounts()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetDepartments_ReturnsAllWithEmployeeAndAssetCounts));
        var controller = new DepartmentsController(context);

        // Act
        var actionResult = await controller.GetDepartments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(okResult.Value);

        var depts = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value);
        Assert.Equal(2, depts.Count());

        var it = depts.First(d => d.DepartmentCode == "IT");
        Assert.Equal(1, it.EmployeeCount); // Employee 1
        Assert.Equal(1, it.AssignedAssetCount); // Asset 2 (In-Use)

        var hr = depts.First(d => d.DepartmentCode == "HR");
        Assert.Equal(0, hr.EmployeeCount); // Employee 2 is Resigned, not Active
        Assert.Equal(0, hr.AssignedAssetCount);
    }

    [Fact]
    public async Task CreateDepartment_WithManagerID_SavesBothManagerNameAndID()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateDepartment_WithManagerID_SavesBothManagerNameAndID));
        var controller = new DepartmentsController(context);

        var dto = new CreateDepartmentDto
        {
            DepartmentCode = "FIN",
            DepartmentName = "Phòng Tài Chính",
            ManagerName = "Phạm Tài Chính",
            ManagerID = 1 // Employee 1 as manager
        };

        // Act
        var result = await controller.CreateDepartment(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dept = Assert.IsType<Department>(okResult.Value);
        Assert.Equal("FIN", dept.DepartmentCode); // Uppercased
        Assert.Equal("Phạm Tài Chính", dept.ManagerName);
        Assert.Equal(1, dept.ManagerID);
    }

    [Fact]
    public async Task CreateDepartment_DuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateDepartment_DuplicateCode_ReturnsBadRequest));
        var controller = new DepartmentsController(context);

        var dto = new CreateDepartmentDto
        {
            DepartmentCode = "IT", // Already exists
            DepartmentName = "Duplicate IT"
        };

        // Act
        var result = await controller.CreateDepartment(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateDepartment_ValidData_UpdatesCorrectly()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateDepartment_ValidData_UpdatesCorrectly));
        var controller = new DepartmentsController(context);

        var dto = new CreateDepartmentDto
        {
            DepartmentCode = "IT-UPDATED",
            DepartmentName = "Phòng CNTT (Cập nhật)",
            ManagerName = "New Manager Name",
            Description = "Mô tả mới"
        };

        // Act
        var result = await controller.UpdateDepartment(1, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Departments.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("IT-UPDATED", updated.DepartmentCode);
        Assert.Equal("Phòng CNTT (Cập nhật)", updated.DepartmentName);
        Assert.Equal("New Manager Name", updated.ManagerName);
    }

    [Fact]
    public async Task UpdateDepartment_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateDepartment_NotFound_Returns404));
        var controller = new DepartmentsController(context);

        var dto = new CreateDepartmentDto
        {
            DepartmentCode = "NOEXIST",
            DepartmentName = "Non-existent"
        };

        // Act
        var result = await controller.UpdateDepartment(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDepartment_DuplicateCodeOnDifferentDept_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateDepartment_DuplicateCodeOnDifferentDept_ReturnsBadRequest));
        var controller = new DepartmentsController(context);

        // Try to set Department 1's code to "HR" (belongs to Department 2)
        var dto = new CreateDepartmentDto
        {
            DepartmentCode = "HR",
            DepartmentName = "Trying to steal HR code"
        };

        // Act
        var result = await controller.UpdateDepartment(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteDepartment_EmptyDepartment_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteDepartment_EmptyDepartment_Succeeds));
        var controller = new DepartmentsController(context);

        // Add empty department
        context.Departments.Add(new Department
        {
            DepartmentID = 100,
            DepartmentCode = "EMPTY",
            DepartmentName = "Empty Department"
        });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteDepartment(100);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(await context.Departments.AnyAsync(d => d.DepartmentID == 100));
    }

    [Fact]
    public async Task DeleteDepartment_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteDepartment_NotFound_Returns404));
        var controller = new DepartmentsController(context);

        // Act
        var result = await controller.DeleteDepartment(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
