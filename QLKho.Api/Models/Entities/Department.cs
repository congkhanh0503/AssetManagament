using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

[Table("Departments")]
public class Department
{
    [Key]
    public int DepartmentID { get; set; }

    [Required]
    [MaxLength(50)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DepartmentName { get; set; } = string.Empty;

    public int? ManagerID { get; set; }

    [MaxLength(255)]
    public string? ManagerName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ManagerID")]
    public virtual Employee? Manager { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
