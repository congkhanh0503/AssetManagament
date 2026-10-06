using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class Supplier
{
    [Key]
    public int SupplierID { get; set; }

    [Required]
    [MaxLength(50)]
    public string SupplierCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
