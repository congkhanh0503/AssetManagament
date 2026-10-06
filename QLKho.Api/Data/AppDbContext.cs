using Microsoft.EntityFrameworkCore;
using QLKho.Api.Models.Entities;

namespace QLKho.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetHandoverHistory> AssetHandoverHistories => Set<AssetHandoverHistory>();
    public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EmployeeHistory> EmployeeHistories => Set<EmployeeHistory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderDeviceItem> OrderDeviceItems => Set<OrderDeviceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Department -> Manager (Employee)
        modelBuilder.Entity<Department>()
            .HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerID)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Employee -> Department
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Asset relationships
        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Supplier)
            .WithMany(s => s.Assets)
            .HasForeignKey(a => a.SupplierID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.CurrentHolder)
            .WithMany(e => e.HeldAssets)
            .HasForeignKey(a => a.CurrentHolderID)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure AssetHandoverHistory relationships
        modelBuilder.Entity<AssetHandoverHistory>()
            .HasOne(h => h.Asset)
            .WithMany(a => a.HandoverHistories)
            .HasForeignKey(h => h.AssetID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetHandoverHistory>()
            .HasOne(h => h.FromEmployee)
            .WithMany()
            .HasForeignKey(h => h.FromEmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetHandoverHistory>()
            .HasOne(h => h.ToEmployee)
            .WithMany()
            .HasForeignKey(h => h.ToEmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetHandoverHistory>()
            .HasOne(h => h.FromDepartment)
            .WithMany()
            .HasForeignKey(h => h.FromDepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssetHandoverHistory>()
            .HasOne(h => h.ToDepartment)
            .WithMany()
            .HasForeignKey(h => h.ToDepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AssetMaintenance relationships
        modelBuilder.Entity<AssetMaintenance>()
            .HasOne(m => m.Asset)
            .WithMany(a => a.Maintenances)
            .HasForeignKey(m => m.AssetID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetMaintenance>()
            .HasOne(m => m.Reporter)
            .WithMany()
            .HasForeignKey(m => m.ReportedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Orders relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Supplier)
            .WithMany()
            .HasForeignKey(o => o.SupplierID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Category)
            .WithMany()
            .HasForeignKey(oi => oi.CategoryID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderDeviceItem>()
            .HasOne(od => od.Order)
            .WithMany(o => o.Devices)
            .HasForeignKey(od => od.OrderID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDeviceItem>()
            .HasOne(od => od.OrderItem)
            .WithMany(oi => oi.Devices)
            .HasForeignKey(od => od.OrderItemID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDeviceItem>()
            .HasOne(od => od.Asset)
            .WithMany()
            .HasForeignKey(od => od.AssetID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderDeviceItem>()
            .HasIndex(od => new { od.OrderID, od.SerialNumber })
            .IsUnique();

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Order)
            .WithMany()
            .HasForeignKey(a => a.OrderID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
