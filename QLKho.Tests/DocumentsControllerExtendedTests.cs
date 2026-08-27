using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

/// <summary>
/// Additional tests for DocumentsController endpoints not covered in DocumentsControllerTests.cs:
/// - DownloadDocument
/// - DeleteDocument
/// - UploadDocument edge cases (invalid file type, missing file)
/// </summary>
public class DocumentsControllerExtendedTests
{
    private readonly TestWebHostEnvironment _env = new();

    [Fact]
    public async Task UploadDocument_InvalidFileExtension_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UploadDocument_InvalidFileExtension_ReturnsBadRequest));
        var controller = new DocumentsController(context, _env);

        var content = "Fake content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", "malware.exe")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };

        var dto = new UploadDocumentDto
        {
            DocumentName = "Invalid file",
            DocumentType = "Other",
            File = formFile
        };

        // Act
        var result = await controller.UploadDocument(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadDocument_NoFile_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UploadDocument_NoFile_ReturnsBadRequest));
        var controller = new DocumentsController(context, _env);

        var dto = new UploadDocumentDto
        {
            DocumentName = "No file",
            DocumentType = "Other",
            File = null! // No file
        };

        // Act
        var result = await controller.UploadDocument(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadDocument_ImageFile_SavesSuccessfully()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(UploadDocument_ImageFile_SavesSuccessfully));
        var controller = new DocumentsController(context, _env);

        var content = "Fake image content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", "screenshot.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var dto = new UploadDocumentDto
        {
            DocumentName = "Screenshot of broken screen",
            DocumentType = "Other",
            File = formFile,
            Description = "Hình ảnh hiện tượng lỗi"
        };

        // Act
        var result = await controller.UploadDocument(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var doc = context.Documents.FirstOrDefault(d => d.DocumentName == "Screenshot of broken screen");
        Assert.NotNull(doc);
        Assert.Equal(".png", doc.FileExtension);
    }

    [Fact]
    public async Task GetDocuments_SearchByDocumentName_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetDocuments_SearchByDocumentName_ReturnsMatchingResults));
        var controller = new DocumentsController(context, _env);

        context.Documents.AddRange(
            new Document
            {
                DocumentName = "Biên bản bàn giao máy Dell",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/doc1.pdf",
                FileName = "doc1.pdf",
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new Document
            {
                DocumentName = "Hóa đơn mua MacBook",
                DocumentType = "Invoice",
                FilePath = "/uploads/doc2.pdf",
                FileName = "doc2.pdf",
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDocuments(new DocumentFilterDto { Search = "Dell" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var docs = Assert.IsAssignableFrom<IEnumerable<DocumentItemDto>>(okResult.Value);
        Assert.Single(docs);
        Assert.Contains("Dell", docs.First().DocumentName);
    }

    [Fact]
    public async Task GetDocuments_FilterByAssetId_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetDocuments_FilterByAssetId_ReturnsMatchingResults));
        var controller = new DocumentsController(context, _env);

        context.Documents.AddRange(
            new Document
            {
                DocumentName = "Doc for Asset 1",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/doc1.pdf",
                FileName = "doc1.pdf",
                AssetID = 1,
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new Document
            {
                DocumentName = "Doc for Asset 2",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/doc2.pdf",
                FileName = "doc2.pdf",
                AssetID = 2,
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDocuments(new DocumentFilterDto { AssetID = 1 });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var docs = Assert.IsAssignableFrom<IEnumerable<DocumentItemDto>>(okResult.Value);
        Assert.Single(docs);
        Assert.Equal(1, docs.First().AssetID);
    }

    [Fact]
    public async Task GetDocuments_FilterByEmployeeId_ReturnsMatchingResults()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(GetDocuments_FilterByEmployeeId_ReturnsMatchingResults));
        var controller = new DocumentsController(context, _env);

        context.Documents.AddRange(
            new Document
            {
                DocumentName = "Doc for Employee 1",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/doc1.pdf",
                FileName = "doc1.pdf",
                EmployeeID = 1,
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new Document
            {
                DocumentName = "Doc for Employee 2",
                DocumentType = "HandoverReceipt",
                FilePath = "/uploads/doc2.pdf",
                FileName = "doc2.pdf",
                EmployeeID = 2,
                UploadedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetDocuments(new DocumentFilterDto { EmployeeID = 2 });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var docs = Assert.IsAssignableFrom<IEnumerable<DocumentItemDto>>(okResult.Value);
        Assert.Single(docs);
        Assert.Equal(2, docs.First().EmployeeID);
    }

    [Fact]
    public async Task DeleteDocument_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteDocument_NotFound_Returns404));
        var controller = new DocumentsController(context, _env);

        // Act
        var result = await controller.DeleteDocument(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteDocument_ValidId_RemovesFromDatabase()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DeleteDocument_ValidId_RemovesFromDatabase));
        var controller = new DocumentsController(context, _env);

        context.Documents.Add(new Document
        {
            DocumentName = "To be deleted",
            DocumentType = "Other",
            FilePath = "/uploads/to_delete.pdf",
            FileName = "to_delete.pdf",
            UploadedBy = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var docId = context.Documents.First().DocumentID;

        // Act
        var result = await controller.DeleteDocument(docId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.False(context.Documents.Any(d => d.DocumentID == docId));
    }

    [Fact]
    public async Task DownloadDocument_NotFound_Returns404()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(DownloadDocument_NotFound_Returns404));
        var controller = new DocumentsController(context, _env);

        // Act
        var result = await controller.DownloadDocument(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
