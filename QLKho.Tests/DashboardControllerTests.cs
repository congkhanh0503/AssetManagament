using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using Xunit;

namespace QLKho.Tests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetKpis_CalculatesCorrectStats()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetKpis_CalculatesCorrectStats));
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetKpis();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var kpi = Assert.IsType<DashboardKpiDto>(okResult.Value);

        Assert.Equal(3, kpi.TotalAssets);
        Assert.Equal(1, kpi.InUseAssets);
        Assert.Equal(1, kpi.AvailableAssets);
        Assert.Equal(1, kpi.BrokenAssets);
        Assert.Equal(2, kpi.TotalEmployees);
    }
}
