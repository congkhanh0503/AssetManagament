using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

public class EmployeesControllerTests
{
    [Fact]
    public async Task GetEmployees_ReturnsListWithHoldingAssetCount()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetEmployees_ReturnsListWithHoldingAssetCount));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.GetEmployees(null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeItemDto>>(okResult.Value);
        Assert.Equal(2, employees.Count());

        var emp1 = employees.First(e => e.EmployeeCode == "EMP001");
        Assert.Equal(1, emp1.HoldingAssetCount); // Đang giữ 1 máy (MacBook Pro M3)
    }

    [Fact]
    public async Task GetLeaveAlerts_DetectsEmployeesWithLeaveDate_AndActiveAccountsOrAssets()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetLeaveAlerts_DetectsEmployeesWithLeaveDate_AndActiveAccountsOrAssets));
        var controller = new EmployeesController(context);

        // Act
        var result = await controller.GetLeaveAlerts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var alerts = Assert.IsAssignableFrom<IEnumerable<EmployeeAlertDto>>(okResult.Value);

        // EMP002 đã nghỉ việc và QAD_Status == "Available" -> Phải nằm trong danh sách cảnh báo
        Assert.Contains(alerts, a => a.EmployeeCode == "EMP002");
    }

    [Fact]
    public async Task UpdateAccountStatus_Updates4AccountsSuccessfully()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateAccountStatus_Updates4AccountsSuccessfully));
        var controller = new EmployeesController(context);

        var dto = new UpdateAccountStatusDto
        {
            QAD_Status = "Disable",
            OA_Status = "Disable",
            Email_Status = "Deleted",
            AD_Status = "Disable"
        };

        // Act
        var result = await controller.UpdateAccountStatus(2, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var emp = await context.Employees.FindAsync(2);
        Assert.NotNull(emp);
        Assert.Equal("Disable", emp.QAD_Status);
        Assert.Equal("Deleted", emp.Email_Status);
    }

    [Fact]
    public async Task ImportBulkEmployees_ValidData_InsertsNewEmployees()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ImportBulkEmployees_ValidData_InsertsNewEmployees));
        var controller = new EmployeesController(context);

        var items = new List<ImportEmployeeItemDto>
        {
            new ImportEmployeeItemDto
            {
                EmployeeCode = "EMP-IMP-001",
                FullName = "Hoàng Kim Ngân",
                DepartmentName = "Phòng Công Nghệ Thông Tin",
                Title = "DevOps Engineer",
                Email = "ngan.hoang@company.com"
            }
        };

        // Act
        var result = await controller.ImportBulkEmployees(items);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bulkRes = Assert.IsType<BulkImportResultDto>(okResult.Value);

        Assert.Equal(1, bulkRes.SuccessCount);
        Assert.True(context.Employees.Any(e => e.EmployeeCode == "EMP-IMP-001"));
    }

    [Fact]
    public async Task UpdateEmployee_PreservesAccountStatuses_WhenNotSpecified()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UpdateEmployee_PreservesAccountStatuses_WhenNotSpecified));
        var controller = new EmployeesController(context);

        var emp = new Employee
        {
            EmployeeCode = "EMP-ACC-001",
            FullName = "Nguyễn Văn Toàn",
            DepartmentID = 1,
            QAD_Status = "Available",
            OA_Status = "Available",
            Email_Status = "Available",
            AD_Status = "Available",
            Status = "Active"
        };
        context.Employees.Add(emp);
        await context.SaveChangesAsync();

        // Giả sử DTO chỉ cập nhật số điện thoại và email, không truyền lại 4 trường tài khoản (hoặc null)
        var updateDto = new UpdateEmployeeDto
        {
            EmployeeCode = "EMP-ACC-001",
            FullName = "Nguyễn Văn Toàn (Senior)",
            DepartmentID = 1,
            Phone = "0988776655",
            Email = "toan.nguyen@company.com",
            Status = "Active"
            // QAD_Status, OA_Status, Email_Status, AD_Status đều là null
        };

        // Act
        var result = await controller.UpdateEmployee(emp.EmployeeID, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Employees.FindAsync(emp.EmployeeID);
        Assert.NotNull(updated);
        Assert.Equal("Nguyễn Văn Toàn (Senior)", updated.FullName);
        Assert.Equal("0988776655", updated.Phone);
        // Kiểm tra 4 trạng thái tài khoản không bị reset về "Disable"
        Assert.Equal("Available", updated.QAD_Status);
        Assert.Equal("Available", updated.OA_Status);
        Assert.Equal("Available", updated.Email_Status);
        Assert.Equal("Available", updated.AD_Status);
    }
}
