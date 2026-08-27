using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportAssetsCsvAsync(AssetFilterDto filter);
    Task<byte[]> ExportEmployeesCsvAsync(EmployeeFilterDto filter);
    Task<byte[]> ExportHandoverHistoriesCsvAsync(HandoverHistoryFilterDto filter);
    Task<byte[]> ExportEmployeeHistoriesCsvAsync(EmployeeHistoryFilterDto filter);
}
