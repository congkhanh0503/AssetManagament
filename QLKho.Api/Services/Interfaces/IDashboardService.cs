using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetKpisAsync();
    Task<PeriodHighlightDto> GetPeriodHighlightsAsync(string period = "month");
    Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync();
    Task<List<DepartmentDistributionDto>> GetDepartmentDistributionAsync();
    Task<List<BrokenAssetHighlightDto>> GetBrokenHighlightsAsync();
    Task<BrandDefectSummaryDto> GetBrandDefectStatsAsync(string month = "all");
    Task<List<TopHoldingEmployeeDto>> GetTopHoldingEmployeesAsync(int limit = 6);
}
