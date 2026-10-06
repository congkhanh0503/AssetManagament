using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly IImportService _importService;
    private readonly IExportService _exportService;

    public AssetsController(
        IAssetService assetService,
        IImportService importService,
        IExportService exportService)
    {
        _assetService = assetService;
        _importService = importService;
        _exportService = exportService;
    }

    // GET: api/assets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetItemDto>>> GetAssets([FromQuery] AssetFilterDto filter)
    {
        var assets = await _assetService.GetAssetsAsync(filter);
        return Ok(assets);
    }

    // GET: api/assets/export-csv
    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] AssetFilterDto filter)
    {
        var bytes = await _exportService.ExportAssetsCsvAsync(filter);
        return File(bytes, "text/csv; charset=utf-8", $"DanhSachTaiSan_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    // GET: api/assets/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDetailDto>> GetAsset(int id)
    {
        var asset = await _assetService.GetAssetByIdAsync(id);
        if (asset == null) return NotFound(new { message = "Không tìm thấy thiết bị" });
        return Ok(asset);
    }

    // POST: api/assets
    [HttpPost]
    public async Task<ActionResult<AssetItemDto>> CreateAsset([FromBody] CreateAssetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _assetService.CreateAssetAsync(dto);
            return CreatedAtAction(nameof(GetAsset), new { id = created.AssetID }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/assets/5
    [HttpPut("{id}")]
    public async Task<ActionResult<AssetItemDto>> UpdateAsset(int id, [FromBody] UpdateAssetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _assetService.UpdateAssetAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy thiết bị" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/assets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var success = await _assetService.DeleteAssetAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy thiết bị" });
        return NoContent();
    }

    // POST: api/assets/bundle-assign (Cấp phát trọn gói ACID)
    [HttpPost("bundle-assign")]
    public async Task<IActionResult> BundleAssign([FromBody] BundleAssignDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _assetService.BundleAssignAsync(dto);
            return Ok(new { message = "Cấp phát trọn gói thành công!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/assign
    [HttpPost("assign")]
    public async Task<IActionResult> AssignAsset([FromBody] AssignAssetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _assetService.AssignAssetAsync(dto);
            return Ok(new { message = "Cấp phát thiết bị thành công!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/transfer
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferAsset([FromBody] TransferAssetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _assetService.TransferAssetAsync(dto);
            return Ok(new { message = "Điều chuyển thiết bị thành công!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/return
    [HttpPost("return")]
    public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _assetService.ReturnAssetAsync(dto);
            return Ok(new { message = "Thu hồi thiết bị về kho thành công!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/report-issue
    [HttpPost("report-issue")]
    public async Task<IActionResult> ReportIssue([FromBody] ReportIssueDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _assetService.ReportIssueAsync(dto);
            return Ok(new { message = "Ghi nhận báo hỏng / gửi bảo trì thành công!" });
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
            return BadRequest(new { message = msg });
        }
    }

    // POST: api/assets/accessory-lot (Nhập lô phụ kiện tự sinh mã)
    [HttpPost("accessory-lot")]
    public async Task<ActionResult<List<AssetItemDto>>> CreateAccessoryLot([FromBody] CreateAccessoryLotDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _assetService.CreateAccessoryLotAsync(dto);
            return Ok(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/assets/accessory-lot/preview
    [HttpGet("accessory-lot/preview")]
    public async Task<ActionResult<AccessoryLotPreviewDto>> GetAccessoryLotPreview([FromQuery] int categoryId, [FromQuery] string brand)
    {
        try
        {
            var preview = await _assetService.GetAccessoryLotPreviewAsync(categoryId, brand);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/import-bulk
    [HttpPost("import-bulk")]
    public async Task<ActionResult<BulkImportResultDto>> ImportBulk([FromBody] List<ImportAssetItemDto> items)
    {
        try
        {
            var result = await _importService.ImportAssetsBulkAsync(items);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/assets/import-handover
    [HttpPost("import-handover")]
    public async Task<ActionResult<ImportHandoverResultDto>> ImportHandover([FromBody] List<ImportHandoverItemDto> items)
    {
        try
        {
            var result = await _importService.ImportHandoverBulkAsync(items);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/assets/handover-history
    [HttpGet("handover-history")]
    public async Task<ActionResult<IEnumerable<HandoverHistoryItemDto>>> GetHandoverHistories([FromQuery] HandoverHistoryFilterDto filter)
    {
        var histories = await _assetService.GetHandoverHistoriesAsync(filter);
        return Ok(histories);
    }

    // GET: api/assets/handover-history/export-csv
    [HttpGet("handover-history/export-csv")]
    public async Task<IActionResult> ExportHandoverHistoryCsv([FromQuery] HandoverHistoryFilterDto filter)
    {
        var bytes = await _exportService.ExportHandoverHistoriesCsvAsync(filter);
        return File(bytes, "text/csv; charset=utf-8", $"LichSuBanGiao_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}
