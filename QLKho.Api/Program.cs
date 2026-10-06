using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using QLKho.Api.Data;

// Hỗ trợ lưu trữ DateTime linh hoạt (UTC/Unspecified) trên PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Cố định cổng Backend API là http://0.0.0.0:5000 (cho phép truy cập cả từ localhost và Docker Network)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// 1. Cấu hình DbContext kết nối PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=QLKhoAssetDB;Username=postgres;Password=postgres;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// 2. Cấu hình Controllers và JSON serializer
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 3. Cấu hình CORS mở rộng
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Đăng ký các Backend Services (Clean Architecture)
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IAssetService, QLKho.Api.Services.Implementations.AssetService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IEmployeeService, QLKho.Api.Services.Implementations.EmployeeService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IImportService, QLKho.Api.Services.Implementations.ImportService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IExportService, QLKho.Api.Services.Implementations.ExportService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IDashboardService, QLKho.Api.Services.Implementations.DashboardService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IDepartmentService, QLKho.Api.Services.Implementations.DepartmentService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.ICategoryService, QLKho.Api.Services.Implementations.CategoryService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.ISupplierService, QLKho.Api.Services.Implementations.SupplierService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IBrandService, QLKho.Api.Services.Implementations.BrandService>();
builder.Services.AddScoped<QLKho.Api.Services.Interfaces.IOrderService, QLKho.Api.Services.Implementations.OrderService>();

// 5. Cấu hình Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Tự động kiểm tra và khởi tạo Database Schema nếu chưa có (PostgreSQL Auto-Migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // 1. Tự động tạo các bảng và bổ sung cột theo chuẩn database/mssql_schema_and_seed.sql
        var createTablesSql = @"
            -- 1. Bảng Bộ phận / Phòng ban (Departments)
            CREATE TABLE IF NOT EXISTS departments (
                department_id SERIAL PRIMARY KEY,
                department_code VARCHAR(50) NOT NULL UNIQUE,
                department_name VARCHAR(255) NOT NULL,
                manager_id INT,
                manager_name VARCHAR(255),
                description VARCHAR(500),
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 2. Bảng Nhân viên (Employees)
            CREATE TABLE IF NOT EXISTS employees (
                employee_id SERIAL PRIMARY KEY,
                employee_code VARCHAR(50) NOT NULL UNIQUE,
                full_name VARCHAR(255) NOT NULL,
                english_name VARCHAR(255),
                department_id INT REFERENCES departments(department_id) ON DELETE SET NULL,
                title VARCHAR(150),
                email VARCHAR(255),
                phone VARCHAR(50),
                join_date DATE NOT NULL DEFAULT CURRENT_DATE,
                leave_date DATE,
                qad_status VARCHAR(20) NOT NULL DEFAULT 'Disable',
                oa_status VARCHAR(20) NOT NULL DEFAULT 'Disable',
                email_status VARCHAR(20) NOT NULL DEFAULT 'Disable',
                ad_status VARCHAR(20) NOT NULL DEFAULT 'Disable',
                status VARCHAR(30) NOT NULL DEFAULT 'Active',
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 3. Bảng Phân loại Tài sản (AssetCategories)
            CREATE TABLE IF NOT EXISTS asset_categories (
                category_id SERIAL PRIMARY KEY,
                category_code VARCHAR(50) NOT NULL UNIQUE,
                category_name VARCHAR(255) NOT NULL,
                description VARCHAR(500),
                is_active BOOLEAN NOT NULL DEFAULT TRUE
            );

            -- 4. Bảng Nhà cung cấp (Suppliers)
            CREATE TABLE IF NOT EXISTS suppliers (
                supplier_id SERIAL PRIMARY KEY,
                supplier_code VARCHAR(50) NOT NULL UNIQUE,
                supplier_name VARCHAR(255) NOT NULL,
                contact_person VARCHAR(150),
                phone VARCHAR(50),
                email VARCHAR(255),
                address VARCHAR(500),
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 5. Bảng Thương hiệu (Brands)
            CREATE TABLE IF NOT EXISTS brands (
                brand_id SERIAL PRIMARY KEY,
                brand_name VARCHAR(100) NOT NULL UNIQUE,
                origin_country VARCHAR(100),
                description VARCHAR(255),
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 6. Bảng Thiết bị / Tài sản (Assets)
            CREATE TABLE IF NOT EXISTS assets (
                asset_id SERIAL PRIMARY KEY,
                asset_code VARCHAR(100) NOT NULL UNIQUE,
                asset_name VARCHAR(255) NOT NULL,
                category_id INT NOT NULL REFERENCES asset_categories(category_id) ON DELETE RESTRICT,
                brand VARCHAR(100),
                specifications VARCHAR(500),
                material_code VARCHAR(100),
                serial_number VARCHAR(150),
                purchase_date DATE,
                warranty_expire_date DATE,
                supplier_id INT REFERENCES suppliers(supplier_id) ON DELETE SET NULL,
                status VARCHAR(30) NOT NULL DEFAULT 'Available',
                current_holder_id INT REFERENCES employees(employee_id) ON DELETE SET NULL,
                warehouse_location VARCHAR(255) DEFAULT 'Kho IT - Kệ A1',
                note TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 7. Bảng Lịch sử Cấp phát & Thu hồi (AssetHandoverHistory)
            CREATE TABLE IF NOT EXISTS asset_handover_histories (
                history_id SERIAL PRIMARY KEY,
                asset_id INT NOT NULL REFERENCES assets(asset_id) ON DELETE CASCADE,
                action_type VARCHAR(50) NOT NULL,
                from_employee_id INT REFERENCES employees(employee_id) ON DELETE SET NULL,
                to_employee_id INT REFERENCES employees(employee_id) ON DELETE SET NULL,
                from_department_id INT REFERENCES departments(department_id) ON DELETE SET NULL,
                to_department_id INT REFERENCES departments(department_id) ON DELETE SET NULL,
                from_location VARCHAR(255),
                to_location VARCHAR(255),
                action_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                condition_status VARCHAR(255),
                note TEXT,
                handover_code VARCHAR(100),
                created_by VARCHAR(100)
            );

            -- 8. Bảng Quản lý Sửa chữa / Bảo hành (AssetMaintenance)
            CREATE TABLE IF NOT EXISTS asset_maintenances (
                maintenance_id SERIAL PRIMARY KEY,
                asset_id INT NOT NULL REFERENCES assets(asset_id) ON DELETE CASCADE,
                issue_description TEXT NOT NULL,
                reported_by INT REFERENCES employees(employee_id) ON DELETE SET NULL,
                sent_date DATE NOT NULL DEFAULT CURRENT_DATE,
                expected_return_date DATE,
                actual_return_date DATE,
                cost NUMERIC(18,2) DEFAULT 0,
                vendor_name VARCHAR(255),
                status VARCHAR(30) NOT NULL DEFAULT 'In-Progress',
                result_note TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 9. Bảng Người dùng hệ thống (Users)
            CREATE TABLE IF NOT EXISTS users (
                user_id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL UNIQUE,
                password VARCHAR(255) NOT NULL,
                full_name VARCHAR(100) NOT NULL,
                role VARCHAR(50) NOT NULL DEFAULT 'Admin',
                avatar VARCHAR(255),
                email VARCHAR(100),
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                last_login_at TIMESTAMPTZ
            );

            -- 10. Bảng Tài liệu & Biên bản scan (Documents)
            CREATE TABLE IF NOT EXISTS documents (
                document_id SERIAL PRIMARY KEY,
                document_name VARCHAR(255) NOT NULL,
                document_type VARCHAR(50) NOT NULL DEFAULT 'HandoverReceipt',
                file_path VARCHAR(500) NOT NULL,
                file_name VARCHAR(255) NOT NULL,
                file_size BIGINT DEFAULT 0,
                file_extension VARCHAR(20) DEFAULT '.pdf',
                content_type VARCHAR(100) DEFAULT 'application/pdf',
                asset_id INT REFERENCES assets(asset_id) ON DELETE CASCADE,
                employee_id INT REFERENCES employees(employee_id) ON DELETE CASCADE,
                department_id INT REFERENCES departments(department_id) ON DELETE SET NULL,
                uploaded_by VARCHAR(100) DEFAULT 'Admin',
                description VARCHAR(500),
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 11. Bảng Lịch sử biến động nhân sự (EmployeeHistories)
            CREATE TABLE IF NOT EXISTS employee_histories (
                employee_history_id SERIAL PRIMARY KEY,
                employee_id INT NOT NULL REFERENCES employees(employee_id) ON DELETE CASCADE,
                action_type VARCHAR(50) NOT NULL,
                title VARCHAR(200) NOT NULL,
                description VARCHAR(1000),
                old_value VARCHAR(500),
                new_value VARCHAR(500),
                performed_by VARCHAR(100) DEFAULT 'Hệ Thống',
                action_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 12. Bảng Đơn hàng (Orders)
            CREATE TABLE IF NOT EXISTS orders (
                order_id SERIAL PRIMARY KEY,
                order_code VARCHAR(50) NOT NULL UNIQUE,
                order_name VARCHAR(255) NOT NULL,
                is_project_based BOOLEAN NOT NULL DEFAULT FALSE,
                project_name VARCHAR(255),
                supplier_id INT REFERENCES suppliers(supplier_id) ON DELETE SET NULL,
                supplier_name VARCHAR(255),
                order_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                expected_delivery_date TIMESTAMPTZ,
                actual_delivery_date TIMESTAMPTZ,
                status VARCHAR(30) NOT NULL DEFAULT 'Pending',
                note TEXT,
                created_by VARCHAR(100) DEFAULT 'Admin',
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 13. Bảng Dòng loại thiết bị trong đơn (OrderItems)
            CREATE TABLE IF NOT EXISTS order_items (
                order_item_id SERIAL PRIMARY KEY,
                order_id INT NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
                category_id INT REFERENCES asset_categories(category_id) ON DELETE SET NULL,
                category_name VARCHAR(100),
                model_name VARCHAR(255) NOT NULL,
                brand VARCHAR(100),
                specifications VARCHAR(500),
                expected_quantity INT NOT NULL DEFAULT 1,
                received_quantity INT NOT NULL DEFAULT 0,
                unit_price NUMERIC(18,2),
                note VARCHAR(500),
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- 14. Bảng Thiết bị cụ thể đã nhập vào đơn (OrderDeviceItems)
            CREATE TABLE IF NOT EXISTS order_device_items (
                device_item_id SERIAL PRIMARY KEY,
                order_id INT NOT NULL REFERENCES orders(order_id) ON DELETE CASCADE,
                order_item_id INT NOT NULL REFERENCES order_items(order_item_id) ON DELETE CASCADE,
                asset_code VARCHAR(100) NOT NULL,
                serial_number VARCHAR(150),
                asset_name VARCHAR(255) NOT NULL,
                category_id INT REFERENCES asset_categories(category_id) ON DELETE SET NULL,
                brand VARCHAR(100),
                specifications VARCHAR(500),
                warehouse_location VARCHAR(255) DEFAULT 'Kho IT - Kệ A1',
                is_transferred_to_asset BOOLEAN NOT NULL DEFAULT FALSE,
                asset_id INT REFERENCES assets(asset_id) ON DELETE SET NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            -- =========================================================================
            -- TỰ ĐỘNG BỔ SUNG CỘT CHO TẤT CẢ CÁC BẢNG (ĐỀ PHÒNG DATABASE CŨ BỊ THIẾU CỘT)
            -- =========================================================================
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS department_code VARCHAR(50) DEFAULT 'PB';
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS department_name VARCHAR(255) DEFAULT '';
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS manager_name VARCHAR(255);
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS manager_id INT;
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS description VARCHAR(500);
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE;
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE departments ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE employees ADD COLUMN IF NOT EXISTS employee_code VARCHAR(50);
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS full_name VARCHAR(255) DEFAULT '';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS english_name VARCHAR(255);
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS department_id INT;
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS title VARCHAR(150);
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS email VARCHAR(255);
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS phone VARCHAR(50);
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS join_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS leave_date TIMESTAMPTZ;
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS qad_status VARCHAR(20) DEFAULT 'Disable';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS oa_status VARCHAR(20) DEFAULT 'Disable';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS email_status VARCHAR(20) DEFAULT 'Disable';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS ad_status VARCHAR(20) DEFAULT 'Disable';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS status VARCHAR(30) DEFAULT 'Active';
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE employees ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE asset_categories ADD COLUMN IF NOT EXISTS category_code VARCHAR(50) DEFAULT 'DM';
            ALTER TABLE asset_categories ADD COLUMN IF NOT EXISTS category_name VARCHAR(255) DEFAULT '';
            ALTER TABLE asset_categories ADD COLUMN IF NOT EXISTS description VARCHAR(500);
            ALTER TABLE asset_categories ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE;

            ALTER TABLE brands ADD COLUMN IF NOT EXISTS brand_name VARCHAR(100) DEFAULT '';
            ALTER TABLE brands ADD COLUMN IF NOT EXISTS origin_country VARCHAR(100);
            ALTER TABLE brands ADD COLUMN IF NOT EXISTS description VARCHAR(255);
            ALTER TABLE brands ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS supplier_code VARCHAR(50) DEFAULT 'NCC';
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS supplier_name VARCHAR(255) DEFAULT '';
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS contact_person VARCHAR(150);
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS phone VARCHAR(50);
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS phone_number VARCHAR(50);
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS email VARCHAR(255);
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS address VARCHAR(500);
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE;
            ALTER TABLE suppliers ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE assets ADD COLUMN IF NOT EXISTS asset_code VARCHAR(100) DEFAULT '';
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS asset_name VARCHAR(255) DEFAULT '';
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS category_id INT;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS brand VARCHAR(100);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS specifications VARCHAR(500);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS material_code VARCHAR(100);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS serial_number VARCHAR(150);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS purchase_date TIMESTAMPTZ;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS warranty_expire_date TIMESTAMPTZ;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS supplier_id INT;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS status VARCHAR(30) DEFAULT 'Available';
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS current_holder_id INT;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS warehouse_location VARCHAR(255) DEFAULT 'Kho IT - Kệ A1';
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS note TEXT;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS order_id INT;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS order_code VARCHAR(100);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS project_name VARCHAR(255);
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE assets ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS asset_id INT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS action_type VARCHAR(50) DEFAULT 'Assign';
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS from_employee_id INT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS to_employee_id INT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS from_department_id INT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS to_department_id INT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS from_location VARCHAR(255);
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS to_location VARCHAR(255);
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS action_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS condition_status VARCHAR(255);
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS note TEXT;
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS handover_code VARCHAR(100);
            ALTER TABLE asset_handover_histories ADD COLUMN IF NOT EXISTS created_by VARCHAR(100);

            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS asset_id INT;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS issue_description TEXT DEFAULT '';
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS reported_by INT;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS sent_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS expected_return_date TIMESTAMPTZ;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS actual_return_date TIMESTAMPTZ;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS cost NUMERIC(18,2) DEFAULT 0;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS vendor_name VARCHAR(255);
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS status VARCHAR(30) DEFAULT 'In-Progress';
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS result_note TEXT;
            ALTER TABLE asset_maintenances ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE users ADD COLUMN IF NOT EXISTS username VARCHAR(50) DEFAULT '';
            ALTER TABLE users ADD COLUMN IF NOT EXISTS password VARCHAR(255) DEFAULT '';
            ALTER TABLE users ADD COLUMN IF NOT EXISTS full_name VARCHAR(100) DEFAULT '';
            ALTER TABLE users ADD COLUMN IF NOT EXISTS role VARCHAR(50) DEFAULT 'Admin';
            ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar VARCHAR(255);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS email VARCHAR(100);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE;
            ALTER TABLE users ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE users ADD COLUMN IF NOT EXISTS last_login_at TIMESTAMPTZ;

            ALTER TABLE documents ADD COLUMN IF NOT EXISTS document_name VARCHAR(255) DEFAULT '';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS document_type VARCHAR(50) DEFAULT 'HandoverReceipt';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS file_path VARCHAR(500) DEFAULT '';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS file_name VARCHAR(255) DEFAULT '';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS file_size BIGINT DEFAULT 0;
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS file_extension VARCHAR(20) DEFAULT '.pdf';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS content_type VARCHAR(100) DEFAULT 'application/pdf';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS asset_id INT;
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS employee_id INT;
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS department_id INT;
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS uploaded_by VARCHAR(100) DEFAULT 'Admin';
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS description VARCHAR(500);
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE documents ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS employee_id INT;
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS action_type VARCHAR(50) DEFAULT '';
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS title VARCHAR(200) DEFAULT '';
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS description VARCHAR(1000) DEFAULT '';
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS old_value VARCHAR(500);
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS new_value VARCHAR(500);
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS performed_by VARCHAR(100) DEFAULT 'Hệ Thống';
            ALTER TABLE employee_histories ADD COLUMN IF NOT EXISTS action_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;
        ";

        db.Database.ExecuteSqlRaw(createTablesSql);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Auto-Create Notice]: {ex.Message}");
    }

    try
    {
        // 2. Seed danh mục Phòng Ban ban đầu nếu bảng rỗng
        if (!db.Departments.Any())
        {
            var defaultDepts = new List<QLKho.Api.Models.Entities.Department>
            {
                new() { DepartmentName = "AE", DepartmentCode = "AE", Description = "AE", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "EHS", DepartmentCode = "EHS", Description = "EHS", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Facility", DepartmentCode = "FACILITY", Description = "Facility", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Facility & EHS", DepartmentCode = "FACILITY_EHS", Description = "Facility & EHS", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Finance", DepartmentCode = "FINANCE", Description = "Finance", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "HR&Admin", DepartmentCode = "HR_ADMIN", Description = "HR&Admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "IE", DepartmentCode = "IE", Description = "IE", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "IT", DepartmentCode = "IT", Description = "Bộ phận IT", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Maintenance", DepartmentCode = "MAINTENANCE", Description = "Maintenance", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Management", DepartmentCode = "MANAGEMENT", Description = "Management", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "ME", DepartmentCode = "ME", Description = "ME", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "NPI", DepartmentCode = "NPI", Description = "NPI", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "PE", DepartmentCode = "PE", Description = "PE", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Production", DepartmentCode = "PRODUCTION", Description = "Production", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Project Management", DepartmentCode = "PROJECT_MANAGEMENT", Description = "Project Management", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Purchase", DepartmentCode = "PURCHASE", Description = "Purchase", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Purchasing", DepartmentCode = "PURCHASING", Description = "Purchasing", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "QA", DepartmentCode = "QA", Description = "QA", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Quality", DepartmentCode = "QUALITY", Description = "Quality", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "SCM", DepartmentCode = "SCM", Description = "SCM", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "SCM & Purchase", DepartmentCode = "SCM_PURCHASE", Description = "SCM & Purchase", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "SCM & Purchasing", DepartmentCode = "SCM_PURCHASING", Description = "SCM & Purchasing", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "TE", DepartmentCode = "TE", Description = "TE", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            db.Departments.AddRange(defaultDepts);
            db.SaveChanges();
        }

        // 3. Seed danh mục Loại Thiết Bị ban đầu nếu bảng rỗng
        if (!db.AssetCategories.Any())
        {
            var defaultCategories = new List<QLKho.Api.Models.Entities.AssetCategory>
            {
                new() { CategoryName = "Laptop & Máy tính xách tay", CategoryCode = "LAPTOP", Description = "Laptop Dell, HP, Lenovo, Macbook...", IsActive = true },
                new() { CategoryName = "Máy tính để bàn (PC / Desktop)", CategoryCode = "DESKTOP", Description = "Máy tính để bàn, Workstation, All-in-one...", IsActive = true },
                new() { CategoryName = "Màn hình máy tính (Monitor)", CategoryCode = "MONITOR", Description = "Màn hình vi tính các loại", IsActive = true },
                new() { CategoryName = "Chuột máy tính (Mouse)", CategoryCode = "MOUSE", Description = "Chuột quang có dây, không dây", IsActive = true },
                new() { CategoryName = "Bàn phím máy tính (Keyboard)", CategoryCode = "KEYBOARD", Description = "Bàn phím cơ, văn phòng có dây/không dây", IsActive = true },
                new() { CategoryName = "Máy in & Thiết bị Scan", CategoryCode = "PRINTER", Description = "Máy in laser, in màu, máy scan tài liệu", IsActive = true },
                new() { CategoryName = "Thiết bị mạng & Khác", CategoryCode = "NETWORK", Description = "Switch, Router, Wifi, Phụ kiện khác", IsActive = true }
            };
            db.AssetCategories.AddRange(defaultCategories);
            db.SaveChanges();
        }

        // 4. Seed danh mục Hãng sản xuất ban đầu nếu bảng rỗng
        if (!db.Brands.Any())
        {
            var defaultBrands = new List<QLKho.Api.Models.Entities.Brand>
            {
                new() { BrandName = "Dell", OriginCountry = "Mỹ", Description = "Laptop, Máy trạm, Màn hình, PC đồng bộ" },
                new() { BrandName = "HP", OriginCountry = "Mỹ", Description = "Laptop ProBook/EliteBook, Máy in, PC" },
                new() { BrandName = "Lenovo", OriginCountry = "Trung Quốc", Description = "ThinkPad, ThinkCentre, Legion" },
                new() { BrandName = "Apple", OriginCountry = "Mỹ", Description = "MacBook, iMac, Mac Mini, iPad" },
                new() { BrandName = "Asus", OriginCountry = "Đài Loan", Description = "Laptop ZenBook, ExpertBook, ROG" },
                new() { BrandName = "Acer", OriginCountry = "Đài Loan", Description = "Laptop Aspire, Swift, Predator" },
                new() { BrandName = "Logitech", OriginCountry = "Thụy Sĩ", Description = "Chuột, Bàn phím, Webcam, Tai nghe" },
                new() { BrandName = "Genius", OriginCountry = "Đài Loan", Description = "Chuột quang văn phòng, Bàn phím phổ thông" },
                new() { BrandName = "Fuhlen", OriginCountry = "Đức/TQ", Description = "Chuột quang, Phím cơ văn phòng" },
                new() { BrandName = "DareU", OriginCountry = "Trung Quốc", Description = "Chuột, Bàn phím cơ, Tai nghe" },
                new() { BrandName = "Samsung", OriginCountry = "Hàn Quốc", Description = "Màn hình vi tính, Ổ cứng SSD" },
                new() { BrandName = "LG", OriginCountry = "Hàn Quốc", Description = "Màn hình vi tính UltraFine/UltraGear" },
                new() { BrandName = "Cisco", OriginCountry = "Mỹ", Description = "Thiết bị mạng Switch, Router, Firewall" },
                new() { BrandName = "Brother", OriginCountry = "Nhật Bản", Description = "Máy in laser, Máy scan tài liệu" },
                new() { BrandName = "Canon", OriginCountry = "Nhật Bản", Description = "Máy in phun, Máy in laser" }
            };
            db.Brands.AddRange(defaultBrands);
            db.SaveChanges();
        }

        // 5. Tự động khởi tạo tài khoản Admin mặc định (tk: admin, mk: 123456)
        var adminUser = db.Users.FirstOrDefault(u => u.Username.ToLower() == "admin");
        if (adminUser == null)
        {
            db.Users.Add(new QLKho.Api.Models.Entities.User
            {
                Username = "admin",
                Password = "123456",
                FullName = "Quản Trị Viên Kho (Admin)",
                Role = "Admin",
                Email = "admin@qlkho.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        // 6. Tự động khởi tạo tài khoản Nhân Sự (HR) mặc định (tk: hr, mk: 123456)
        var hrUser = db.Users.FirstOrDefault(u => u.Username.ToLower() == "hr");
        if (hrUser == null)
        {
            db.Users.Add(new QLKho.Api.Models.Entities.User
            {
                Username = "hr",
                Password = "123456",
                FullName = "Chuyên Viên Nhân Sự (HR)",
                Role = "HR",
                Email = "hr@qlkho.local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        // 7. Tự động khởi tạo dữ liệu lịch sử cấp phát ban đầu nếu bảng đang rỗng
        if (!db.AssetHandoverHistories.Any())
        {
            var inUseAssets = db.Assets.Include(a => a.CurrentHolder).Where(a => a.Status == "In-Use" && a.CurrentHolderID != null).ToList();
            var histories = new List<QLKho.Api.Models.Entities.AssetHandoverHistory>();
            var now = DateTime.UtcNow;

            int dayOffset = 1;
            foreach (var asset in inUseAssets)
            {
                histories.Add(new QLKho.Api.Models.Entities.AssetHandoverHistory
                {
                    AssetID = asset.AssetID,
                    ActionType = "Assign",
                    ToEmployeeID = asset.CurrentHolderID,
                    ToDepartmentID = asset.CurrentHolder?.DepartmentID,
                    ToLocation = asset.CurrentHolder?.DepartmentID != null ? "Bàn làm việc NV" : "Văn phòng",
                    ActionDate = now.AddDays(-dayOffset),
                    ConditionStatus = "Máy hoạt động tốt, cấp mới",
                    Note = $"Cấp phát thiết bị {asset.AssetName} cho {asset.CurrentHolder?.FullName}",
                    CreatedBy = "Admin IT"
                });
                dayOffset = (dayOffset + 3) % 28; // Phân bổ trải dài từ tuần này đến tháng này
            }

            // Thêm 1 bản ghi thu hồi mẫu nếu có máy Available
            var availAsset = db.Assets.FirstOrDefault(a => a.Status == "Available");
            var sampleEmp = db.Employees.FirstOrDefault();
            if (availAsset != null && sampleEmp != null)
            {
                histories.Add(new QLKho.Api.Models.Entities.AssetHandoverHistory
                {
                    AssetID = availAsset.AssetID,
                    ActionType = "Return",
                    FromEmployeeID = sampleEmp.EmployeeID,
                    ToLocation = "Kho IT - Kệ A1",
                    ActionDate = now.AddDays(-4),
                    ConditionStatus = "Đã vệ sinh, kiểm tra OK",
                    Note = "Thu hồi máy nhập lại kho",
                    CreatedBy = "Admin IT"
                });
            }

            if (histories.Count > 0)
            {
                db.AssetHandoverHistories.AddRange(histories);
                db.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Auto-Update Notice]: {ex.Message}");
    }
}

// 5. Cấu hình Static Files để phục vụ preview PDF
app.UseStaticFiles();

// 6. Cấu hình Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QLKho Asset API v1");
    c.RoutePrefix = "swagger"; // Đường dẫn truy cập: http://localhost:5000/swagger
});

app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

// Endpoint root
app.MapGet("/", () => Results.Ok(new
{
    status = "Online",
    system = "QLKho Asset Management API",
    framework = "ASP.NET Core 8/10",
    swaggerUrl = "http://localhost:5000/swagger",
    time = DateTime.UtcNow
}));

app.Run();
