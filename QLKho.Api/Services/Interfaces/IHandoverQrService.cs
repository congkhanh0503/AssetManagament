using Microsoft.AspNetCore.Http;
using QLKho.Api.Models.DTOs;

namespace QLKho.Api.Services.Interfaces;

public interface IHandoverQrService
{
    Task<HandoverAutoSplitResultDto> ProcessBatchHandoverPdfAsync(IFormFile pdfFile, string? uploadedBy = "Admin");
}
