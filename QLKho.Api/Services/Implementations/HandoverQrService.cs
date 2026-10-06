using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf.IO;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UglyToad.PdfPig;
using ZXing;
using ZXing.Common;

namespace QLKho.Api.Services.Implementations;

public class HandoverQrService : IHandoverQrService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly string _handoverDir;

    public HandoverQrService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _handoverDir = Path.Combine(webRoot, "uploads", "handovers");
        if (!Directory.Exists(_handoverDir))
        {
            Directory.CreateDirectory(_handoverDir);
        }
    }

    public async Task<HandoverAutoSplitResultDto> ProcessBatchHandoverPdfAsync(IFormFile pdfFile, string? uploadedBy = "Admin")
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return new HandoverAutoSplitResultDto
            {
                Success = false,
                Message = "File PDF tải lên không hợp lệ hoặc rỗng."
            };
        }

        var ext = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
        if (ext != ".pdf")
        {
            return new HandoverAutoSplitResultDto
            {
                Success = false,
                Message = "Hệ thống chỉ hỗ trợ xử lý file định dạng PDF (.pdf)."
            };
        }

        byte[] pdfBytes;
        using (var memoryStream = new MemoryStream())
        {
            await pdfFile.CopyToAsync(memoryStream);
            pdfBytes = memoryStream.ToArray();
        }

        var result = new HandoverAutoSplitResultDto();
        var recognizedItems = new List<HandoverAutoSplitItemDto>();
        var unrecognizedPages = new List<int>();

        int totalPages = 0;

        // 1. Dùng UglyToad.PdfPig để đọc từng trang và giải mã mã QR
        using (var pdfDoc = PdfDocument.Open(pdfBytes))
        {
            totalPages = pdfDoc.NumberOfPages;
            result.TotalPages = totalPages;

            for (int pageNum = 1; pageNum <= totalPages; pageNum++)
            {
                var page = pdfDoc.GetPage(pageNum);
                string? detectedQrText = null;

                // Lấy tất cả ảnh trong trang PDF
                var pageImages = page.GetImages().ToList();

                foreach (var img in pageImages)
                {
                    detectedQrText = TryDecodeQrFromPdfImage(img);
                    if (!string.IsNullOrWhiteSpace(detectedQrText))
                    {
                        break; // Đã tìm thấy mã QR trên trang này
                    }
                }

                if (!string.IsNullOrWhiteSpace(detectedQrText))
                {
                    ParseQrData(detectedQrText, out string empCode, out string assetCode);

                    recognizedItems.Add(new HandoverAutoSplitItemDto
                    {
                        PageNumber = pageNum,
                        RawQrContent = detectedQrText,
                        EmployeeCode = empCode,
                        AssetCode = assetCode
                    });
                }
                else
                {
                    unrecognizedPages.Add(pageNum);
                }
            }
        }

        // 2. Dùng PdfSharpCore để cắt từng trang thành file PDF riêng biệt và cập nhật DB
        using (var inputPdf = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import))
        {
            foreach (var item in recognizedItems)
            {
                int pageIdx = item.PageNumber - 1; // 0-based
                if (pageIdx < 0 || pageIdx >= inputPdf.PageCount) continue;

                try
                {
                    using (var singlePdf = new PdfSharpCore.Pdf.PdfDocument())
                    {
                        singlePdf.AddPage(inputPdf.Pages[pageIdx]);

                        string safeEmp = !string.IsNullOrWhiteSpace(item.EmployeeCode) ? item.EmployeeCode.Trim() : "unknown";
                        string safeAsset = !string.IsNullOrWhiteSpace(item.AssetCode) ? item.AssetCode.Trim() : "unknown";
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

                        string fileName = $"handover_{safeEmp}_{safeAsset}_p{item.PageNumber}_{timestamp}.pdf";
                        string fullPath = Path.Combine(_handoverDir, fileName);

                        singlePdf.Save(fullPath);

                        item.FileName = fileName;
                        item.FilePath = $"/uploads/handovers/{fileName}";

                        // 3. Liên kết với Nhân viên và Tài sản trong Database
                        await AttachDocumentToSystemAsync(item, fullPath, uploadedBy);
                        item.Success = true;
                        item.Message = $"Đã tách trang {item.PageNumber} và gán vào hệ thống thành công!";
                    }
                }
                catch (Exception ex)
                {
                    item.Success = false;
                    item.Message = $"Lỗi khi tách trang {item.PageNumber}: {ex.Message}";
                }
            }

            // Đối với các trang không có QR: tách riêng thành file unassigned để IT xem lại nếu muốn
            foreach (var pageNum in unrecognizedPages)
            {
                int pageIdx = pageNum - 1;
                if (pageIdx < 0 || pageIdx >= inputPdf.PageCount) continue;

                try
                {
                    using (var singlePdf = new PdfSharpCore.Pdf.PdfDocument())
                    {
                        singlePdf.AddPage(inputPdf.Pages[pageIdx]);
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                        string fileName = $"unassigned_page_{pageNum}_{timestamp}.pdf";
                        string fullPath = Path.Combine(_handoverDir, fileName);
                        singlePdf.Save(fullPath);

                        result.Items.Add(new HandoverAutoSplitItemDto
                        {
                            PageNumber = pageNum,
                            FileName = fileName,
                            FilePath = $"/uploads/handovers/{fileName}",
                            Success = false,
                            Message = "Không tìm thấy mã QR trên trang này hoặc mã QR bị mờ."
                        });
                    }
                }
                catch { }
            }
        }

        result.Items.AddRange(recognizedItems);
        result.Items = result.Items.OrderBy(i => i.PageNumber).ToList();
        result.UnrecognizedPages = unrecognizedPages;
        result.SuccessCount = recognizedItems.Count(i => i.Success);
        result.FailedCount = result.TotalPages - result.SuccessCount;
        result.Success = result.SuccessCount > 0;
        result.Message = result.SuccessCount > 0
            ? $"Xử lý hoàn tất! Đã tự động nhận diện và gán thành công {result.SuccessCount}/{result.TotalPages} trang biên bản."
            : "Không tìm thấy mã QR hợp lệ nào trên các trang của file PDF.";

        return result;
    }

    private string? TryDecodeQrFromPdfImage(UglyToad.PdfPig.Content.IPdfImage img)
    {
        try
        {
            byte[]? imageBytes = null;
            if (img.TryGetPng(out byte[] png))
            {
                imageBytes = png;
            }
            else
            {
                try
                {
                    imageBytes = img.RawBytes.ToArray();
                }
                catch { }
            }

            if (imageBytes == null || imageBytes.Length == 0) return null;

            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);
            if (image == null) return null;

            // Thử giải mã ở góc bình thường và các góc xoay (trong trường hợp người dùng đặt ngược giấy scan)
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            // Lần 1: Giải mã trực tiếp
            string? decoded = DecodeImageWithZXing(image, reader);
            if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

            // Lần 2: Xoay 90 độ
            image.Mutate(x => x.Rotate(RotateMode.Rotate90));
            decoded = DecodeImageWithZXing(image, reader);
            if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

            // Lần 3: Xoay tiếp 90 độ (180 độ)
            image.Mutate(x => x.Rotate(RotateMode.Rotate90));
            decoded = DecodeImageWithZXing(image, reader);
            if (!string.IsNullOrWhiteSpace(decoded)) return decoded;

            // Lần 4: Xoay tiếp 90 độ (270 độ)
            image.Mutate(x => x.Rotate(RotateMode.Rotate90));
            decoded = DecodeImageWithZXing(image, reader);
            return decoded;
        }
        catch
        {
            return null;
        }
    }

    private static string? DecodeImageWithZXing(Image<Rgba32> image, BarcodeReaderGeneric reader)
    {
        try
        {
            int width = image.Width;
            int height = image.Height;
            byte[] grayBytes = new byte[width * height];
            int offset = 0;

            for (int y = 0; y < height; y++)
            {
                var rowSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    var p = rowSpan[x];
                    grayBytes[offset++] = (byte)((p.R * 299 + p.G * 587 + p.B * 114) / 1000);
                }
            }

            var luminanceSource = new RGBLuminanceSource(grayBytes, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
            var result = reader.Decode(luminanceSource);
            return result?.Text;
        }
        catch
        {
            return null;
        }
    }

    private static void ParseQrData(string qrText, out string empCode, out string assetCode)
    {
        empCode = string.Empty;
        assetCode = string.Empty;

        if (string.IsNullOrWhiteSpace(qrText)) return;

        qrText = qrText.Trim();

        // 1. Thử parse dạng JSON: {"t":"HB","emp":"EMP0123","ast":"AST-0001"}
        if (qrText.StartsWith("{") && qrText.EndsWith("}"))
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(qrText);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("emp", out var empProp) || root.TryGetProperty("empCode", out empProp))
                {
                    empCode = empProp.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("ast", out var astProp) || root.TryGetProperty("assetCode", out astProp) || root.TryGetProperty("asset", out astProp))
                {
                    assetCode = astProp.GetString() ?? string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(empCode) || !string.IsNullOrWhiteSpace(assetCode))
                {
                    return;
                }
            }
            catch { }
        }

        // 2. Thử parse dạng phân cách pipe: HB|EMP0226|AST-00001|20261006
        if (qrText.Contains('|'))
        {
            var parts = qrText.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.StartsWith("EMP", StringComparison.OrdinalIgnoreCase))
                {
                    empCode = p;
                }
                else if (p.StartsWith("AST", StringComparison.OrdinalIgnoreCase) || p.StartsWith("TTH-", StringComparison.OrdinalIgnoreCase) || p.StartsWith("VNIT", StringComparison.OrdinalIgnoreCase))
                {
                    assetCode = p;
                }
            }

            // Nếu chưa tìm thấy nhưng có cấu trúc HB|EMP|AST
            if (string.IsNullOrEmpty(empCode) && parts.Length >= 2)
            {
                empCode = parts[1];
            }
            if (string.IsNullOrEmpty(assetCode) && parts.Length >= 3)
            {
                assetCode = parts[2];
            }

            if (!string.IsNullOrWhiteSpace(empCode) || !string.IsNullOrWhiteSpace(assetCode))
            {
                return;
            }
        }

        // 3. Thử parse dạng key=value: EMP=EMP0226;AST=AST-00001
        if (qrText.Contains('='))
        {
            var pairs = qrText.Split(new[] { ';', ',', '&' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length == 2)
                {
                    if (kv[0].Equals("emp", StringComparison.OrdinalIgnoreCase) || kv[0].Equals("empCode", StringComparison.OrdinalIgnoreCase))
                    {
                        empCode = kv[1];
                    }
                    else if (kv[0].Equals("ast", StringComparison.OrdinalIgnoreCase) || kv[0].Equals("assetCode", StringComparison.OrdinalIgnoreCase))
                    {
                        assetCode = kv[1];
                    }
                }
            }
        }
    }

    private async Task AttachDocumentToSystemAsync(HandoverAutoSplitItemDto item, string fullDiskPath, string? uploadedBy)
    {
        Employee? employee = null;
        if (!string.IsNullOrWhiteSpace(item.EmployeeCode))
        {
            var empCodeClean = item.EmployeeCode.Trim().ToLower();
            employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeCode != null && e.EmployeeCode.ToLower() == empCodeClean);
        }

        Asset? asset = null;
        if (!string.IsNullOrWhiteSpace(item.AssetCode))
        {
            var assetCodeClean = item.AssetCode.Trim().ToLower();
            asset = await _context.Assets
                .FirstOrDefaultAsync(a => 
                    a.AssetCode.ToLower() == assetCodeClean || 
                    (a.MaterialCode != null && a.MaterialCode.ToLower() == assetCodeClean) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower() == assetCodeClean)
                );
        }

        if (employee != null)
        {
            item.EmployeeName = employee.FullName;
        }

        if (asset != null)
        {
            item.AssetName = asset.AssetName;
            item.SerialNumber = asset.SerialNumber;
        }

        // Tạo bản ghi trong bảng Documents
        string docName = asset != null 
            ? $"Biên bản bàn giao - {asset.AssetCode} ({asset.AssetName}) - {employee?.FullName ?? item.EmployeeCode}"
            : $"Biên bản bàn giao - {employee?.FullName ?? item.EmployeeCode}";

        long fileSize = 0;
        try
        {
            fileSize = new FileInfo(fullDiskPath).Length;
        }
        catch { }

        // Kiểm tra xem đã có document loại HandoverReceipt cho cặp Employee & Asset này chưa
        var existingDoc = await _context.Documents
            .FirstOrDefaultAsync(d => 
                d.DocumentType == "HandoverReceipt" &&
                ((employee != null && d.EmployeeID == employee.EmployeeID) || (employee == null && d.EmployeeID == null)) &&
                ((asset != null && d.AssetID == asset.AssetID) || (asset == null && d.AssetID == null))
            );

        if (existingDoc != null)
        {
            existingDoc.DocumentName = docName;
            existingDoc.FilePath = item.FilePath!;
            existingDoc.FileName = item.FileName!;
            existingDoc.FileSize = fileSize;
            existingDoc.UploadedBy = uploadedBy ?? "Auto-Scan";
            existingDoc.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var newDoc = new Document
            {
                DocumentName = docName,
                DocumentType = "HandoverReceipt",
                FilePath = item.FilePath!,
                FileName = item.FileName!,
                FileSize = fileSize,
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                AssetID = asset?.AssetID,
                EmployeeID = employee?.EmployeeID,
                DepartmentID = employee?.DepartmentID,
                UploadedBy = uploadedBy ?? "Auto-Scan",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Documents.Add(newDoc);
        }

        await _context.SaveChangesAsync();
    }
}
