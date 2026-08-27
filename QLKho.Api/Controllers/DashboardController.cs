using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET: api/dashboard/kpis
    [HttpGet("kpis")]
    public async Task<ActionResult<DashboardKpiDto>> GetKpis()
    {
        var kpis = await _dashboardService.GetKpisAsync();
        return Ok(kpis);
    }

    // GET: api/dashboard/highlights?period=week|month
    [HttpGet("highlights")]
    public async Task<ActionResult<PeriodHighlightDto>> GetPeriodHighlights([FromQuery] string period = "month")
    {
        var highlights = await _dashboardService.GetPeriodHighlightsAsync(period);
        return Ok(highlights);
    }

    // GET: api/dashboard/categories
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryDistributionDto>>> GetCategoryDistribution()
    {
        var list = await _dashboardService.GetCategoryDistributionAsync();
        return Ok(list);
    }

    // GET: api/dashboard/departments
    [HttpGet("departments")]
    public async Task<ActionResult<IEnumerable<DepartmentDistributionDto>>> GetDepartmentDistribution()
    {
        var list = await _dashboardService.GetDepartmentDistributionAsync();
        return Ok(list);
    }

    // GET: api/dashboard/broken-highlights
    [HttpGet("broken-highlights")]
    public async Task<ActionResult<IEnumerable<BrokenAssetHighlightDto>>> GetBrokenHighlights()
    {
        var list = await _dashboardService.GetBrokenHighlightsAsync();
        return Ok(list);
    }

    // GET: api/dashboard/brand-defect-stats?month=all|2026-08
    [HttpGet("brand-defect-stats")]
    public async Task<ActionResult<BrandDefectSummaryDto>> GetBrandDefectStats([FromQuery] string month = "all")
    {
        var stats = await _dashboardService.GetBrandDefectStatsAsync(month);
        return Ok(stats);
    }

    // GET: api/dashboard/top-holding-employees
    [HttpGet("top-holding-employees")]
    public async Task<ActionResult<IEnumerable<TopHoldingEmployeeDto>>> GetTopHoldingEmployees([FromQuery] int limit = 6)
    {
        var list = await _dashboardService.GetTopHoldingEmployeesAsync(limit);
        return Ok(list);
    }
}
