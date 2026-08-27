using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class BrandDto
{
    public int BrandID { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? OriginCountry { get; set; }
    public string? Description { get; set; }
    public int AssetCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBrandDto
{
    [Required(ErrorMessage = "Vui lòng nhập tên hãng / thương hiệu")]
    [MaxLength(100, ErrorMessage = "Tên hãng không được vượt quá 100 ký tự")]
    public string BrandName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? OriginCountry { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }
}

public class UpdateBrandDto : CreateBrandDto
{
}

