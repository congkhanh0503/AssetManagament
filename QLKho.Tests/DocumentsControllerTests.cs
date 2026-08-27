using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using Xunit;

namespace QLKho.Tests;

public class DocumentsControllerTests
{
    private readonly TestWebHostEnvironment _env = new();

    [Fact]
    public async Task UploadDocument_ValidPdf_SavesDocumentAndReturnsSuccess()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UploadDocument_ValidPdf_SavesDocumentAndReturnsSuccess));
        var controller = new DocumentsController(context, _env);

        var content = "Fake PDF Content";
        var fileName = "bien_ban_ban_giao_AST001.pdf";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var dto = new UploadDocumentDto
        {
            DocumentName = "Biên Bản Bàn Giao Laptop AST-001",
            DocumentType = "HandoverReceipt",
            File = formFile,
            AssetID = 1,
            EmployeeID = 1,
            Description = "Biên bản có chữ ký 2 bên"
        };

        // Act
        var result = await controller.UploadDocument(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var docInDb = context.Documents.FirstOrDefault(d => d.DocumentName == "Biên Bản Bàn Giao Laptop AST-001");
        Assert.NotNull(docInDb);
        Assert.Equal(".pdf", docInDb.FileExtension);
        Assert.Equal(1, docInDb.AssetID);
    }

    [Fact]
    public async Task GetDocuments_WithFilter_ReturnsMatchingList()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetDocuments_WithFilter_ReturnsMatchingList));
        var controller = new DocumentsController(context, _env);

        // Thêm 2 document mẫu
        context.Documents.AddRange(
            new QLKho.Api.Models.Entities.Document
            {
                DocumentName = "Biên bản bàn giao 1",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/documents/doc1.pdf",
                FileName = "doc1.pdf",
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new QLKho.Api.Models.Entities.Document
            {
                DocumentName = "Hóa đơn mua máy Mac",
                DocumentType = "Invoice",
                FilePath = "/uploads/documents/doc2.pdf",
                FileName = "doc2.pdf",
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDocuments(new DocumentFilterDto { DocumentType = "Invoice" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var docs = Assert.IsAssignableFrom<IEnumerable<DocumentItemDto>>(okResult.Value);
        Assert.Single(docs);
        Assert.Equal("Hóa đơn mua máy Mac", docs.First().DocumentName);
    }
}
