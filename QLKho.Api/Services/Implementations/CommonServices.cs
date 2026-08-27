using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var list = await _context.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return list.Select(d => new DepartmentDto
        {
            DepartmentID = d.DepartmentID,
            DepartmentName = d.DepartmentName,
            DepartmentCode = d.DepartmentCode,
            ManagerName = d.ManagerName,
            Description = d.Description,
            EmployeeCount = d.Employees.Count
        }).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(x => x.DepartmentID == id);

        if (d == null) return null;

        return new DepartmentDto
        {
            DepartmentID = d.DepartmentID,
            DepartmentName = d.DepartmentName,
            DepartmentCode = d.DepartmentCode,
            ManagerName = d.ManagerName,
            Description = d.Description,
            EmployeeCount = d.Employees.Count
        };
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var dept = new Department
        {
            DepartmentName = dto.DepartmentName.Trim(),
            DepartmentCode = dto.DepartmentCode?.Trim(),
            ManagerName = dto.ManagerName?.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(dept.DepartmentID))!;
    }

    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) throw new KeyNotFoundException("Không tìm thấy phòng ban");

        dept.DepartmentName = dto.DepartmentName.Trim();
        dept.DepartmentCode = dto.DepartmentCode?.Trim();
        dept.ManagerName = dto.ManagerName?.Trim();
        dept.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(dept.DepartmentID))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dept = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.DepartmentID == id);
        if (dept == null) return false;

        if (dept.Employees.Any())
            throw new InvalidOperationException($"Không thể xóa phòng ban này vì đang có {dept.Employees.Count} nhân viên trực thuộc!");

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var list = await _context.AssetCategories
            .Include(c => c.Assets)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return list.Select(c => new CategoryDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            CategoryCode = c.CategoryCode,
            Description = c.Description,
            AssetCount = c.Assets.Count
        }).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var c = await _context.AssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(x => x.CategoryID == id);

        if (c == null) return null;

        return new CategoryDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            CategoryCode = c.CategoryCode,
            Description = c.Description,
            AssetCount = c.Assets.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var cat = new AssetCategory
        {
            CategoryName = dto.CategoryName.Trim(),
            CategoryCode = dto.CategoryCode?.Trim(),
            Description = dto.Description?.Trim()
        };

        _context.AssetCategories.Add(cat);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(cat.CategoryID))!;
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var cat = await _context.AssetCategories.FindAsync(id);
        if (cat == null) throw new KeyNotFoundException("Không tìm thấy danh mục loại tài sản");

        cat.CategoryName = dto.CategoryName.Trim();
        cat.CategoryCode = dto.CategoryCode?.Trim();
        cat.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(cat.CategoryID))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cat = await _context.AssetCategories.Include(c => c.Assets).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (cat == null) return false;

        if (cat.Assets.Any())
            throw new InvalidOperationException($"Không thể xóa loại thiết bị này vì đang có {cat.Assets.Count} tài sản liên kết!");

        _context.AssetCategories.Remove(cat);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var list = await _context.Suppliers
            .Include(s => s.Assets)
            .OrderBy(s => s.SupplierName)
            .ToListAsync();

        return list.Select(s => new SupplierDto
        {
            SupplierID = s.SupplierID,
            SupplierName = s.SupplierName,
            ContactPerson = s.ContactPerson,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            AssetCount = s.Assets.Count
        }).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var s = await _context.Suppliers
            .Include(s => s.Assets)
            .FirstOrDefaultAsync(x => x.SupplierID == id);

        if (s == null) return null;

        return new SupplierDto
        {
            SupplierID = s.SupplierID,
            SupplierName = s.SupplierName,
            ContactPerson = s.ContactPerson,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            AssetCount = s.Assets.Count
        };
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
    {
        var sup = new Supplier
        {
            SupplierName = dto.SupplierName.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(sup);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(sup.SupplierID))!;
    }

    public async Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var sup = await _context.Suppliers.FindAsync(id);
        if (sup == null) throw new KeyNotFoundException("Không tìm thấy nhà cung cấp");

        sup.SupplierName = dto.SupplierName.Trim();
        sup.ContactPerson = dto.ContactPerson?.Trim();
        sup.Phone = dto.Phone?.Trim();
        sup.Email = dto.Email?.Trim();
        sup.Address = dto.Address?.Trim();

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(sup.SupplierID))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sup = await _context.Suppliers.Include(s => s.Assets).FirstOrDefaultAsync(s => s.SupplierID == id);
        if (sup == null) return false;

        if (sup.Assets.Any())
            throw new InvalidOperationException($"Không thể xóa nhà cung cấp này vì đang có {sup.Assets.Count} tài sản liên kết!");

        _context.Suppliers.Remove(sup);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;

    public BrandService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandDto>> GetAllAsync()
    {
        var list = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
        var allAssets = await _context.Assets.Select(a => a.Brand).ToListAsync();

        return list.Select(b => new BrandDto
        {
            BrandID = b.BrandID,
            BrandName = b.BrandName,
            OriginCountry = b.OriginCountry,
            Description = b.Description,
            AssetCount = allAssets.Count(a => string.Equals(a, b.BrandName, StringComparison.OrdinalIgnoreCase))
        }).ToList();
    }

    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        var b = await _context.Brands.FindAsync(id);
        if (b == null) return null;

        var assetCount = await _context.Assets.CountAsync(a => a.Brand == b.BrandName);

        return new BrandDto
        {
            BrandID = b.BrandID,
            BrandName = b.BrandName,
            OriginCountry = b.OriginCountry,
            Description = b.Description,
            AssetCount = assetCount
        };
    }

    public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
    {
        var brand = new Brand
        {
            BrandName = dto.BrandName.Trim(),
            OriginCountry = dto.OriginCountry?.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(brand.BrandID))!;
    }

    public async Task<BrandDto> UpdateAsync(int id, UpdateBrandDto dto)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null) throw new KeyNotFoundException("Không tìm thấy thương hiệu");

        brand.BrandName = dto.BrandName.Trim();
        brand.OriginCountry = dto.OriginCountry?.Trim();
        brand.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(brand.BrandID))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null) return false;

        var hasAssets = await _context.Assets.AnyAsync(a => a.Brand == brand.BrandName);
        if (hasAssets)
            throw new InvalidOperationException($"Không thể xóa thương hiệu '{brand.BrandName}' vì đang có thiết bị sử dụng thương hiệu này!");

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
        return true;
    }
}
