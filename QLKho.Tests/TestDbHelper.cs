using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.Entities;

namespace QLKho.Tests;

public static class TestDbHelper
{
    public static AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        SeedData(context);
        return context;
    }

    private static void SeedData(AppDbContext context)
    {
        if (context.Departments.Any()) return;

        var itDept = new Department
        {
            DepartmentID = 1,
            DepartmentCode = "IT",
            DepartmentName = "Phòng Công Nghệ Thông Tin",
            ManagerName = "Trần Văn Khoa"
        };
        var hrDept = new Department
        {
            DepartmentID = 2,
            DepartmentCode = "HR",
            DepartmentName = "Phòng Hành Chính Nhân Sự",
            ManagerName = "Nguyễn Thị Hoa"
        };
        context.Departments.AddRange(itDept, hrDept);

        var laptopCat = new AssetCategory
        {
            CategoryID = 1,
            CategoryCode = "LAPTOP",
            CategoryName = "Máy tính xách tay"
        };
        var monitorCat = new AssetCategory
        {
            CategoryID = 2,
            CategoryCode = "MONITOR",
            CategoryName = "Màn hình"
        };
        context.AssetCategories.AddRange(laptopCat, monitorCat);

        var supplier = new Supplier
        {
            SupplierID = 1,
            SupplierCode = "SUP-FPT",
            SupplierName = "FPT Services",
            Phone = "02873007300"
        };
        context.Suppliers.Add(supplier);

        var emp1 = new Employee
        {
            EmployeeID = 1,
            EmployeeCode = "EMP001",
            FullName = "Nguyễn Văn A",
            DepartmentID = 1,
            Title = "IT Lead",
            Email = "a.nguyen@company.com",
            JoinDate = new DateTime(2023, 1, 1),
            QAD_Status = "Available",
            OA_Status = "Available",
            Email_Status = "Available",
            AD_Status = "Available",
            Status = "Active"
        };
        var emp2 = new Employee
        {
            EmployeeID = 2,
            EmployeeCode = "EMP002",
            FullName = "Lê Thị B",
            DepartmentID = 2,
            Title = "HR Specialist",
            Email = "b.le@company.com",
            JoinDate = new DateTime(2023, 5, 1),
            LeaveDate = new DateTime(2026, 8, 10), // Nghỉ việc
            QAD_Status = "Available", // Chưa disable -> Cần cảnh báo
            OA_Status = "Disable",
            Email_Status = "Disable",
            AD_Status = "Disable",
            Status = "Resigned"
        };
        context.Employees.AddRange(emp1, emp2);

        var asset1 = new Asset
        {
            AssetID = 1,
            AssetCode = "AST-LT-001",
            AssetName = "Dell XPS 13",
            CategoryID = 1,
            Brand = "Dell",
            SerialNumber = "SN-DLL-001",
            SupplierID = 1,
            Status = "Available",
            WarehouseLocation = "Kho IT - Kệ A1"
        };
        var asset2 = new Asset
        {
            AssetID = 2,
            AssetCode = "AST-LT-002",
            AssetName = "MacBook Pro M3",
            CategoryID = 1,
            Brand = "Apple",
            SerialNumber = "SN-APL-002",
            SupplierID = 1,
            Status = "In-Use",
            CurrentHolderID = 1,
            WarehouseLocation = "Kho IT - Kệ A1"
        };
        var asset3 = new Asset
        {
            AssetID = 3,
            AssetCode = "AST-MN-001",
            AssetName = "Dell UltraSharp 27",
            CategoryID = 2,
            Brand = "Dell",
            SerialNumber = "SN-DLL-MN1",
            SupplierID = 1,
            Status = "Broken",
            WarehouseLocation = "Kho IT - Kệ Chờ Sửa"
        };
        context.Assets.AddRange(asset1, asset2, asset3);

        context.SaveChanges();
    }
}

public class TestWebHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
{
    public string WebRootPath { get; set; } = Path.GetTempPath();
    public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "QLKho.Tests";
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Development";
}
