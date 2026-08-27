using Microsoft.AspNetCore.Http;
using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeItemDto>> GetEmployeesAsync(EmployeeFilterDto filter);
    Task<EmployeeItemDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeAssetsResponseDto?> GetEmployeeAssetsAsync(int employeeId);
    Task<EmployeeItemDto> CreateEmployeeAsync(CreateEmployeeDto dto);
    Task<EmployeeItemDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
    Task<bool> DeleteEmployeeAsync(int id);

    // Nghiệp vụ Cảnh báo & Trạng thái tài khoản
    Task<List<LeaveAlertItemDto>> GetLeaveAlertsAsync();
    Task<List<MissingHandoverAlertDto>> GetMissingHandoverAlertsAsync();
    Task<bool> UpdateAccountStatusAsync(int employeeId, UpdateAccountStatusDto dto);

    // Nghiệp vụ File PDF Biên bản bàn giao
    Task<string> UploadHandoverPdfAsync(int employeeId, int? assetId, IFormFile file);
    Task<(byte[] Bytes, string ContentType, string FileName)?> GetHandoverPdfAsync(int employeeId, int? assetId);
    Task<bool> DeleteHandoverPdfAsync(int employeeId, int? assetId);

    // Nghiệp vụ Lịch sử biến động nhân sự
    Task<List<EmployeeHistoryDto>> GetHistoriesAsync(EmployeeHistoryFilterDto filter);
    Task<EmployeeHistoryDto> CreateHistoryAsync(CreateEmployeeHistoryDto dto);
}
