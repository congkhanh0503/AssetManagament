using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IOrderService
{
    Task<List<OrderSummaryDto>> GetAllOrdersAsync(string? status = null, string? keyword = null);
    Task<OrderDetailDto?> GetOrderByIdAsync(int orderId);
    Task<OrderDetailDto> CreateOrderAsync(CreateOrderDto dto, string? createdBy = "Admin");
    Task<OrderDetailDto?> UpdateOrderAsync(int orderId, UpdateOrderDto dto);
    Task<bool> DeleteOrderAsync(int orderId);

    Task<OrderDeviceItemDto> AddDeviceToOrderAsync(int orderId, AddDeviceToOrderDto dto);
    Task<List<OrderDeviceItemDto>> BulkAddDevicesToOrderAsync(int orderId, BulkAddDevicesToOrderDto dto);
    Task<bool> RemoveDeviceFromOrderAsync(int orderId, int deviceItemId);

    Task<SyncOrderToAssetsResultDto> SyncOrderToAssetsAsync(int orderId);
}
