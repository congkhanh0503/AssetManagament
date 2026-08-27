using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IAssetService
{
    Task<List<AssetItemDto>> GetAssetsAsync(AssetFilterDto filter);
    Task<AssetDetailDto?> GetAssetByIdAsync(int id);
    Task<AssetItemDto> CreateAssetAsync(CreateAssetDto dto);
    Task<AssetItemDto> UpdateAssetAsync(int id, UpdateAssetDto dto);
    Task<bool> DeleteAssetAsync(int id);

    // Nghiệp vụ Cấp phát trọn gói (Bundle Assign)
    Task<bool> BundleAssignAsync(BundleAssignDto dto);
    Task<bool> AssignAssetAsync(AssignAssetDto dto);
    Task<bool> TransferAssetAsync(TransferAssetDto dto);
    Task<bool> ReturnAssetAsync(ReturnAssetDto dto);
    Task<bool> ReportIssueAsync(ReportIssueDto dto);

    // Nghiệp vụ Nhập lô phụ kiện tự động sinh mã
    Task<List<AssetItemDto>> CreateAccessoryLotAsync(CreateAccessoryLotDto dto);
    Task<AccessoryLotPreviewDto> GetAccessoryLotPreviewAsync(int categoryId, string brand);

    // Nghiệp vụ Lịch sử bàn giao
    Task<List<HandoverHistoryItemDto>> GetHandoverHistoriesAsync(HandoverHistoryFilterDto filter);
}
