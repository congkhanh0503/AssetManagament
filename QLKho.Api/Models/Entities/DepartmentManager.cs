using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.Entities;

// Danh sách người có chức vụ manager, dùng để chọn trưởng bộ phận cho phòng ban
public class DepartmentManager
{
    [Key]
    public int ManagerID { get; set; }

    public int? EmployeeID { get; set; }

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
