using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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
    options.UseNpgsql(connectionString));

// 2. Cấu hình Controllers và JSON serializer
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 3. Cấu hình CORS để cho phép Frontend Vue 3 gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
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
        db.Database.EnsureCreated();

        // Seed danh mục Phòng Ban ban đầu nếu bảng rỗng
        if (!db.Departments.Any())
        {
            var defaultDepts = new List<QLKho.Api.Models.Entities.Department>
            {
                new() { DepartmentName = "Phòng Công Nghệ Thông Tin", DepartmentCode = "IT", Description = "Phòng IT & Quản trị hạ tầng hệ thống", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Phòng Hành Chính Nhân Sự", DepartmentCode = "HR", Description = "Phòng Nhân sự, Tuyển dụng và Hành chính", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Phòng Kế Toán - Tài Chính", DepartmentCode = "ACC", Description = "Phòng Kế toán, Thu chi và Tài chính", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Phòng Kinh Doanh & Tiếp Thị", DepartmentCode = "SALES", Description = "Phòng Sales & Marketing", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Ban Giám Đốc", DepartmentCode = "BOD", Description = "Ban điều hành & Lãnh đạo công ty", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { DepartmentName = "Phòng Ban Chung", DepartmentCode = "PB_CHUNG", Description = "Phòng ban mặc định cho nhân sự chưa phân bổ", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            db.Departments.AddRange(defaultDepts);
            db.SaveChanges();
        }

        // Seed danh mục Loại Thiết Bị ban đầu nếu bảng rỗng
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

        // Seed danh mục Hãng sản xuất ban đầu nếu bảng rỗng
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

        // Tự động khởi tạo tài khoản Admin mặc định (tk: admin, mk: 123456)
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

        // Tự động khởi tạo tài khoản Nhân Sự (HR) mặc định (tk: hr, mk: 123456)
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

        // Tự động khởi tạo dữ liệu lịch sử cấp phát ban đầu nếu bảng đang rỗng
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

app.UseCors("AllowVueApp");

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
