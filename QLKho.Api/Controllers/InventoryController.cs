using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Implementations;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _service;

    public InventoryController(InventoryService service)
    {
        _service = service;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<InventorySessionDto>>> GetSessions()
        => Ok(await _service.GetSessionsAsync());

    [HttpPost("sessions")]
    public async Task<ActionResult<InventorySessionDto>> CreateSession([FromBody] CreateInventorySessionDto dto)
    {
        try
        {
            return Ok(await _service.CreateSessionAsync(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sessions/{id:int}/summary")]
    public async Task<ActionResult<InventorySummaryDto>> GetSummary(int id)
    {
        var summary = await _service.GetSummaryAsync(id);
        return summary == null ? NotFound(new { message = "Không tìm thấy đợt kiểm kê." }) : Ok(summary);
    }

    [HttpGet("sessions/{id:int}/items")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetItems(
        int id, [FromQuery] string? department, [FromQuery] string? status, [FromQuery] string? search)
        => Ok(await _service.GetItemsAsync(id, department, status, search));

    [HttpPost("sessions/{id:int}/scan")]
    public async Task<ActionResult<InventoryScanResultDto>> Scan(int id, [FromBody] InventoryScanRequestDto dto)
    {
        var result = await _service.ScanAsync(id, dto.Code);
        return result == null ? NotFound(new { message = "Không tìm thấy đợt kiểm kê." }) : Ok(result);
    }

    [HttpPost("sessions/{id:int}/items/{itemId:int}/unscan")]
    public async Task<IActionResult> Unscan(int id, int itemId)
    {
        try
        {
            var ok = await _service.UnscanAsync(id, itemId);
            if (ok == null) return NotFound(new { message = "Không tìm thấy đợt kiểm kê." });
            return ok == true ? Ok(new { message = "Đã hủy ghi nhận." }) : NotFound(new { message = "Không tìm thấy tài sản trong đợt." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sessions/{id:int}/close")]
    public async Task<ActionResult<InventorySessionDto>> Close(int id)
    {
        var s = await _service.SetStatusAsync(id, close: true);
        return s == null ? NotFound(new { message = "Không tìm thấy đợt kiểm kê." }) : Ok(s);
    }

    [HttpPost("sessions/{id:int}/reopen")]
    public async Task<ActionResult<InventorySessionDto>> Reopen(int id)
    {
        var s = await _service.SetStatusAsync(id, close: false);
        return s == null ? NotFound(new { message = "Không tìm thấy đợt kiểm kê." }) : Ok(s);
    }

    [HttpDelete("sessions/{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteSessionAsync(id) ? Ok(new { message = "Đã xóa đợt kiểm kê." }) : NotFound(new { message = "Không tìm thấy đợt kiểm kê." });
}
