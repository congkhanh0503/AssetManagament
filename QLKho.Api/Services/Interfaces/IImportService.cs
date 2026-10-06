using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IImportService
{
    Task<BulkImportResultDto> ImportAssetsBulkAsync(List<ImportAssetItemDto> items);
    Task<ImportEmployeeResultDto> ImportEmployeesBulkAsync(List<ImportEmployeeRowDto> rows, bool updateExisting);
    Task<ImportHandoverResultDto> ImportHandoverBulkAsync(List<ImportHandoverItemDto> items);
    Task<ImportAccountResultDto> ImportAccountsBulkAsync(ImportAccountRequestDto request);
}
