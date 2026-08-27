using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using Xunit;

namespace QLKho.Tests;

public class DepartmentsControllerTests
{
    [Fact]
    public async Task CreateDepartment_WithManagerNameText_Succeeds()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(CreateDepartment_WithManagerNameText_Succeeds));
        var controller = new DepartmentsController(context);

        var createDto = new CreateDepartmentDto
        {
            DepartmentCode = "SALES",
            DepartmentName = "Phòng Kinh Doanh",
            ManagerName = "Võ Hoàng Nam",
            Description = "Phát triển thị trường"
        };

        // Act
        var result = await controller.CreateDepartment(createDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SALES");
        Assert.NotNull(dept);
        Assert.Equal("Võ Hoàng Nam", dept.ManagerName);
    }

    [Fact]
    public async Task DeleteDepartment_WithActiveEmployees_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteDepartment_WithActiveEmployees_ReturnsBadRequest));
        var controller = new DepartmentsController(context);

        // Act (Department 1 đang có EMP001 trực thuộc)
        var result = await controller.DeleteDepartment(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
