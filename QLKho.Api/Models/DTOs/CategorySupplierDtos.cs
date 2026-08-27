using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class CategoryDto
{
    public int CategoryID { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssetCount { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    public string CategoryCode { get; set; } = string.Empty;

    [Required]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto
{
}

public class SupplierDto
{
    public int SupplierID { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int SuppliedAssetCount { get; set; }
    public int AssetCount { get; set; }
}

public class CreateSupplierDto
{
    [Required]
    public string SupplierCode { get; set; } = string.Empty;

    [Required]
    public string SupplierName { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class UpdateSupplierDto : CreateSupplierDto
{
}

