using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/documents
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentItemDto>>> GetDocuments([FromQuery] DocumentFilterDto filter)
    {
        var query = _context.Documents
            .Include(d => d.Asset)
            .Include(d => d.Employee)
            .Include(d => d.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(search) ||
                d.FileName.ToLower().Contains(search) ||
                (d.Asset != null && d.Asset.AssetCode.ToLower().Contains(search)) ||
                (d.Employee != null && d.Employee.FullName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.DocumentType))
        {
            query = query.Where(d => d.DocumentType == filter.DocumentType);
        }

        if (filter.AssetID.HasValue)
        {
            query = query.Where(d => d.AssetID == filter.AssetID.Value);
        }

        if (filter.EmployeeID.HasValue)
        {
            query = query.Where(d => d.EmployeeID == filter.EmployeeID.Value);
        }

        if (filter.DepartmentID.HasValue)
        {
            query = query.Where(d => d.DepartmentID == filter.DepartmentID.Value);
        }

        var docs = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var result = docs.Select(d => new DocumentItemDto
        {
            DocumentID = d.DocumentID,
            DocumentName = d.DocumentName,
            DocumentType = d.DocumentType,
            DocumentTypeLabel = GetDocumentTypeLabel(d.DocumentType),
            FilePath = d.FilePath,
            FileName = d.FileName,
            FileSize = d.FileSize,
            FileSizeFormatted = FormatFileSize(d.FileSize),
            FileExtension = d.FileExtension,
            ContentType = d.ContentType,
            AssetID = d.AssetID,
            AssetCode = d.Asset?.AssetCode,
            AssetName = d.Asset?.AssetName,
            EmployeeID = d.EmployeeID,
            EmployeeCode = d.Employee?.EmployeeCode,
            EmployeeName = d.Employee?.FullName,
            DepartmentID = d.DepartmentID,
            DepartmentName = d.Department?.DepartmentName,
            UploadedBy = d.UploadedBy,
            Description = d.Description,
            CreatedAt = d.CreatedAt
        });

        return Ok(result);
    }

    // POST: api/documents/upload (Import file PDF)
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file hợp lệ để tải lên." });
        }

        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".png", ".jpg", ".jpeg" };

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Chỉ chấp nhận các file tài liệu định dạng PDF, Word, Excel hoặc hình ảnh." });
        }

        // Tạo thư mục lưu trữ trong wwwroot/uploads/documents/
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "documents");

        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        // Tạo tên file duy nhất tránh trùng lặp
        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(dto.File.FileName)}";
        var physicalPath = Path.Combine(uploadDir, safeFileName);
        var relativeUrl = $"/uploads/documents/{safeFileName}";

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var document = new Document
        {
            DocumentName = dto.DocumentName.Trim(),
            DocumentType = dto.DocumentType,
            FilePath = relativeUrl,
            FileName = dto.File.FileName,
            FileSize = dto.File.Length,
            FileExtension = extension,
            ContentType = dto.File.ContentType ?? "application/octet-stream",
            AssetID = dto.AssetID,
            EmployeeID = dto.EmployeeID,
            DepartmentID = dto.DepartmentID,
            Description = dto.Description,
            UploadedBy = dto.UploadedBy ?? "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Tải lên và lưu trữ tài liệu thành công!",
            documentId = document.DocumentID,
            filePath = document.FilePath
        });
    }

    // GET: api/documents/{id}/download
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null)
            return NotFound(new { message = "Không tìm thấy tài liệu yêu cầu." });

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativePath = doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(webRoot, relativePath);

        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound(new { message = "File vật lý không còn tồn tại trên máy chủ." });
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        return File(bytes, doc.ContentType, doc.FileName);
    }

    // DELETE: api/documents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null)
            return NotFound(new { message = "Không tìm thấy tài liệu cần xóa." });

        // Xóa file vật lý
        try
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(webRoot, relativePath);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Delete file error]: {ex.Message}");
        }

        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa tài liệu thành công!" });
    }

    private static string GetDocumentTypeLabel(string type)
    {
        return type switch
        {
            "HandoverReceipt" => "Biên Bản Bàn Giao",
            "WarrantyReceipt" => "Phiếu Bảo Hành",
            "Invoice" => "Hóa Đơn Mua Hàng",
            "Contract" => "Hợp Đồng Mua Bán",
            "UserManual" => "Hướng Dẫn Sử Dụng",
            _ => "Tài Liệu Khác"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}
