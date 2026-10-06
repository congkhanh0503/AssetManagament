using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IImportService _importService;
    private readonly IExportService _exportService;

    public EmployeesController(
        IEmployeeService employeeService,
        IImportService importService,
        IExportService exportService)
    {
        _employeeService = employeeService;
        _importService = importService;
        _exportService = exportService;
    }

    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeItemDto>>> GetEmployees([FromQuery] EmployeeFilterDto filter)
    {
        var employees = await _employeeService.GetEmployeesAsync(filter);
        return Ok(employees);
    }

    // GET: api/employees/export-csv
    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] EmployeeFilterDto filter)
    {
        var bytes = await _exportService.ExportEmployeesCsvAsync(filter);
        return File(bytes, "text/csv; charset=utf-8", $"DanhSachNhanVien_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    // GET: api/employees/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeItemDto>> GetEmployee(int id)
    {
        var emp = await _employeeService.GetEmployeeByIdAsync(id);
        if (emp == null) return NotFound(new { message = "Không tìm thấy nhân viên" });
        return Ok(emp);
    }

    // GET: api/employees/5/assets
    [HttpGet("{id}/assets")]
    public async Task<ActionResult<EmployeeAssetsResponseDto>> GetEmployeeAssets(int id)
    {
        var result = await _employeeService.GetEmployeeAssetsAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy nhân viên" });
        return Ok(result);
    }


    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeItemDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _employeeService.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.EmployeeID }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("employees_employee_code_key") == true)
            {
                return BadRequest(new { message = "Mã nhân viên này đã tồn tại trong hệ thống. Vui lòng chọn mã khác." });
            }
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    // PUT: api/employees/5
    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeItemDto>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _employeeService.UpdateEmployeeAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy nhân viên" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("employees_employee_code_key") == true)
            {
                return BadRequest(new { message = "Mã nhân viên này đã tồn tại trong hệ thống. Vui lòng chọn mã khác." });
            }
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    // DELETE: api/employees/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var success = await _employeeService.DeleteEmployeeAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy nhân viên" });
        return NoContent();
    }

    // GET: api/employees/alerts/leave (Cảnh báo nghỉ việc)
    [HttpGet("alerts/leave")]
    public async Task<ActionResult<IEnumerable<LeaveAlertItemDto>>> GetLeaveAlerts()
    {
        var alerts = await _employeeService.GetLeaveAlertsAsync();
        return Ok(alerts);
    }

    // GET: api/employees/alerts/missing-handover (Cảnh báo thiếu biên bản)
    [HttpGet("alerts/missing-handover")]
    public async Task<ActionResult<IEnumerable<MissingHandoverAlertDto>>> GetMissingHandoverAlerts()
    {
        var alerts = await _employeeService.GetMissingHandoverAlertsAsync();
        return Ok(alerts);
    }

    // PUT: api/employees/5/account-status
    [HttpPut("{id}/account-status")]
    public async Task<IActionResult> UpdateAccountStatus(int id, [FromBody] UpdateAccountStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _employeeService.UpdateAccountStatusAsync(id, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy nhân viên" });
        return Ok(new { message = "Cập nhật trạng thái tài khoản thành công!" });
    }

    // POST: api/employees/5/handover-pdf
    [HttpPost("{id}/handover-pdf")]
    public async Task<IActionResult> UploadHandoverPdf(int id, [FromForm] UploadHandoverPdfDto dto, [FromQuery] int? assetId = null)
    {
        try
        {
            var targetAssetId = dto.AssetID ?? assetId;
            var url = await _employeeService.UploadHandoverPdfAsync(id, targetAssetId, dto.File);
            return Ok(new { fileUrl = url, message = "Tải lên biên bản PDF thành công!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/employees/5/handover-pdf
    [HttpGet("{id}/handover-pdf")]
    public async Task<IActionResult> GetHandoverPdf(int id, [FromQuery] int? assetId = null)
    {
        var file = await _employeeService.GetHandoverPdfAsync(id, assetId);
        if (file == null) return NotFound(new { message = "Không tìm thấy file biên bản bàn giao PDF" });
        return File(file.Value.Bytes, file.Value.ContentType, file.Value.FileName);
    }

    // DELETE: api/employees/5/handover-pdf
    [HttpDelete("{id}/handover-pdf")]
    public async Task<IActionResult> DeleteHandoverPdf(int id, [FromQuery] int? assetId = null)
    {
        var success = await _employeeService.DeleteHandoverPdfAsync(id, assetId);
        if (!success) return NotFound(new { message = "Không tìm thấy file để xóa" });
        return Ok(new { message = "Đã xóa file biên bản bàn giao PDF thành công!" });
    }

    // POST: api/employees/import-bulk
    [HttpPost("import-bulk")]
    public async Task<ActionResult<ImportEmployeeResultDto>> ImportEmployeesBulk(
        [FromBody] List<ImportEmployeeRowDto> rows,
        [FromQuery] bool updateExisting = false)
    {
        try
        {
            var result = await _importService.ImportEmployeesBulkAsync(rows, updateExisting);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/employees/histories
    [HttpGet("histories")]
    public async Task<ActionResult<IEnumerable<EmployeeHistoryDto>>> GetHistories([FromQuery] EmployeeHistoryFilterDto filter)
    {
        var list = await _employeeService.GetHistoriesAsync(filter);
        return Ok(list);
    }

    // GET: api/employees/histories/export-csv
    [HttpGet("histories/export-csv")]
    public async Task<IActionResult> ExportHistoriesCsv([FromQuery] EmployeeHistoryFilterDto filter)
    {
        var bytes = await _exportService.ExportEmployeeHistoriesCsvAsync(filter);
        return File(bytes, "text/csv; charset=utf-8", $"LichSuBienDongNhanSu_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    // POST: api/employees/histories
    [HttpPost("histories")]
    public async Task<ActionResult<EmployeeHistoryDto>> CreateHistory([FromBody] CreateEmployeeHistoryDto dto)
    {
        var history = await _employeeService.CreateHistoryAsync(dto);
        return Ok(history);
    }

    // POST: api/employees/import-accounts
    [HttpPost("import-accounts")]
    public async Task<ActionResult<ImportAccountResultDto>> ImportAccountsBulk([FromBody] ImportAccountRequestDto request)
    {
        try
        {
            var result = await _importService.ImportAccountsBulkAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
