using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

[Table("Documents")]
public class Document
{
    [Key]
    public int DocumentID { get; set; }

    [Required]
    [MaxLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = "HandoverReceipt"; 
    // HandoverReceipt (Biên bản bàn giao), WarrantyReceipt (Phiếu bảo hành), Invoice (Hóa đơn VAT), Contract (Hợp đồng mua bán), UserManual (HDSD), Other

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(20)]
    public string FileExtension { get; set; } = ".pdf";

    [MaxLength(100)]
    public string ContentType { get; set; } = "application/pdf";

    public int? AssetID { get; set; }
    [ForeignKey("AssetID")]
    public virtual Asset? Asset { get; set; }

    public int? EmployeeID { get; set; }
    [ForeignKey("EmployeeID")]
    public virtual Employee? Employee { get; set; }

    public int? DepartmentID { get; set; }
    [ForeignKey("DepartmentID")]
    public virtual Department? Department { get; set; }

    [MaxLength(100)]
    public string UploadedBy { get; set; } = "Admin";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
