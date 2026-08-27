-- =========================================================================================
-- DATABASE: QLKhoAssetDB (Quản lý Tài sản & Thiết bị IT)
-- DBMS: Microsoft SQL Server (2016+)
-- =========================================================================================

-- 1. Tạo Database nếu chưa tồn tại
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QLKhoAssetDB')
BEGIN
    CREATE DATABASE QLKhoAssetDB;
END
GO

USE QLKhoAssetDB;
GO

-- =========================================================================================
-- XÓA BẢNG CŨ NẾU ĐÃ TỒN TẠI (Theo thứ tự ràng buộc khóa ngoại)
-- =========================================================================================
IF OBJECT_ID(N'dbo.AssetMaintenance', N'U') IS NOT NULL DROP TABLE dbo.AssetMaintenance;
IF OBJECT_ID(N'dbo.AssetHandoverHistory', N'U') IS NOT NULL DROP TABLE dbo.AssetHandoverHistory;
IF OBJECT_ID(N'dbo.Assets', N'U') IS NOT NULL DROP TABLE dbo.Assets;
IF OBJECT_ID(N'dbo.AssetCategories', N'U') IS NOT NULL DROP TABLE dbo.AssetCategories;
IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NOT NULL DROP TABLE dbo.Suppliers;
IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID(N'dbo.Departments', N'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

-- =========================================================================================
-- 2. TẠO CÁC BẢNG MASTER DATA
-- =========================================================================================

-- 2.1 Bảng Bộ phận / Phòng ban (Departments)
CREATE TABLE dbo.Departments (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode NVARCHAR(50) NOT NULL UNIQUE,       -- Mã bộ phận (IT, HR, ACC, SALES,...)
    DepartmentName NVARCHAR(255) NOT NULL,             -- Tên bộ phận
    ManagerID INT NULL,                                 -- ID Trưởng bộ phận (liên kết với Employees)
    ManagerName NVARCHAR(255) NULL,                     -- Tên Trưởng bộ phận (nhập trực tiếp)
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2.2 Bảng Nhân viên (Employees)
CREATE TABLE dbo.Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode NVARCHAR(50) NOT NULL UNIQUE,          -- Mã nhân viên (NV001, EMP102...)
    FullName NVARCHAR(255) NOT NULL,                    -- Họ tên
    DepartmentID INT NOT NULL,                          -- Khóa ngoại tới Departments
    Title NVARCHAR(150) NULL,                           -- Chức danh (Software Engineer, HR Manager...)
    Email NVARCHAR(255) NULL,                           -- Email liên hệ
    Phone NVARCHAR(50) NULL,                            -- Điện thoại
    JoinDate DATE NOT NULL,                             -- Ngày vào công ty
    LeaveDate DATE NULL,                                -- Ngày nghỉ việc (nếu có)
    
    -- Tình trạng các tài khoản kèm theo (Available / Disable / Deleted)
    QAD_Status NVARCHAR(20) NOT NULL DEFAULT 'Disable' 
        CHECK (QAD_Status IN ('Available', 'Disable', 'Deleted')),
    OA_Status NVARCHAR(20) NOT NULL DEFAULT 'Disable' 
        CHECK (OA_Status IN ('Available', 'Disable', 'Deleted')),
    Email_Status NVARCHAR(20) NOT NULL DEFAULT 'Disable' 
        CHECK (Email_Status IN ('Available', 'Disable', 'Deleted')),
    AD_Status NVARCHAR(20) NOT NULL DEFAULT 'Disable' 
        CHECK (AD_Status IN ('Available', 'Disable', 'Deleted')),

    Status NVARCHAR(30) NOT NULL DEFAULT 'Active' 
        CHECK (Status IN ('Active', 'OnLeave', 'Resigned')), -- Trạng thái làm việc
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentID) 
        REFERENCES dbo.Departments(DepartmentID)
);
GO

-- Cập nhật ràng buộc ManagerID trong bảng Departments trỏ đến Employees
ALTER TABLE dbo.Departments
ADD CONSTRAINT FK_Departments_Manager FOREIGN KEY (ManagerID) 
    REFERENCES dbo.Employees(EmployeeID);
GO

-- 2.3 Bảng Nhà cung cấp (Suppliers)
CREATE TABLE dbo.Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierCode NVARCHAR(50) NOT NULL UNIQUE,
    SupplierName NVARCHAR(255) NOT NULL,
    ContactPerson NVARCHAR(150) NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(255) NULL,
    Address NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2.4 Bảng Phân loại Tài sản (Asset Categories)
CREATE TABLE dbo.AssetCategories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode NVARCHAR(50) NOT NULL UNIQUE,          -- LAPTOP, PC, MONITOR, PRINTER, NETWORK...
    CategoryName NVARCHAR(255) NOT NULL,                -- Laptop, Máy tính để bàn, Màn hình, Máy in...
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- 2.5 Bảng Thiết bị / Tài sản (Assets)
CREATE TABLE dbo.Assets (
    AssetID INT IDENTITY(1,1) PRIMARY KEY,
    AssetCode NVARCHAR(100) NOT NULL UNIQUE,            -- Mã tài sản (vd: AST-LT-001, QR/Barcode)
    AssetName NVARCHAR(255) NOT NULL,                   -- Tên tài sản (vd: Laptop Dell Latitude 5420)
    CategoryID INT NOT NULL,                            -- Phân loại: Laptop / Thiết bị
    Brand NVARCHAR(100) NULL,                           -- Thương hiệu: Dell, HP, Apple, Lenovo, Cisco...
    MaterialCode NVARCHAR(100) NULL,                    -- Mã vật tư kế toán
    SerialNumber NVARCHAR(150) NULL,                    -- S/N của nhà sản xuất
    PurchaseDate DATE NULL,                             -- Ngày mua
    WarrantyExpireDate DATE NULL,                       -- Thời gian hết hạn bảo hành
    SupplierID INT NULL,                                -- Nhà cung cấp
    
    -- Tình trạng thiết bị: Available / In-Use / Broken / Maintenance / Disposed
    Status NVARCHAR(30) NOT NULL DEFAULT 'Available' 
        CHECK (Status IN ('Available', 'In-Use', 'Broken', 'Maintenance', 'Disposed')),
        
    CurrentHolderID INT NULL,                           -- Người đang sở hữu (Khóa ngoại tới Employees)
    WarehouseLocation NVARCHAR(255) NULL DEFAULT N'Kho IT - Kệ A1', -- Vị trí khi nằm trong kho
    Note NVARCHAR(MAX) NULL,                            -- Ghi chú cấu hình / tình trạng
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Assets_Categories FOREIGN KEY (CategoryID) 
        REFERENCES dbo.AssetCategories(CategoryID),
    CONSTRAINT FK_Assets_Suppliers FOREIGN KEY (SupplierID) 
        REFERENCES dbo.Suppliers(SupplierID),
    CONSTRAINT FK_Assets_CurrentHolder FOREIGN KEY (CurrentHolderID) 
        REFERENCES dbo.Employees(EmployeeID)
);
GO

-- 2.6 Bảng Lịch sử Cấp phát & Thu hồi (Asset Handover History)
CREATE TABLE dbo.AssetHandoverHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    AssetID INT NOT NULL,
    ActionType NVARCHAR(50) NOT NULL 
        CHECK (ActionType IN ('Assign', 'Return', 'Transfer', 'SendMaintenance', 'ReceiveMaintenance', 'Dispose')), 
        -- Assign: Cấp phát, Return: Thu hồi, Transfer: Điều chuyển, SendMaintenance: Gửi sửa, ReceiveMaintenance: Nhận lại, Dispose: Thanh lý
    
    FromEmployeeID INT NULL,                            -- Người bàn giao / Người trả
    ToEmployeeID INT NULL,                              -- Người nhận cấp phát mới
    FromDepartmentID INT NULL,
    ToDepartmentID INT NULL,
    FromLocation NVARCHAR(255) NULL,
    ToLocation NVARCHAR(255) NULL,
    ActionDate DATETIME2 NOT NULL DEFAULT GETDATE(),    -- Ngày giờ thực hiện
    ConditionStatus NVARCHAR(255) NULL,                 -- Tình trạng máy tại thời điểm giao/nhận (Mới 100%, Xước nhẹ, Lỗi bàn phím...)
    Note NVARCHAR(MAX) NULL,                            -- Ghi chú / Lý do
    CreatedBy NVARCHAR(100) NULL,                       -- Người thực hiện thao tác (Admin/IT)

    CONSTRAINT FK_History_Assets FOREIGN KEY (AssetID) 
        REFERENCES dbo.Assets(AssetID),
    CONSTRAINT FK_History_FromEmp FOREIGN KEY (FromEmployeeID) 
        REFERENCES dbo.Employees(EmployeeID),
    CONSTRAINT FK_History_ToEmp FOREIGN KEY (ToEmployeeID) 
        REFERENCES dbo.Employees(EmployeeID),
    CONSTRAINT FK_History_FromDept FOREIGN KEY (FromDepartmentID) 
        REFERENCES dbo.Departments(DepartmentID),
    CONSTRAINT FK_History_ToDept FOREIGN KEY (ToDepartmentID) 
        REFERENCES dbo.Departments(DepartmentID)
);
GO

-- 2.7 Bảng Quản lý Sửa chữa / Bảo hành (Asset Maintenance)
CREATE TABLE dbo.AssetMaintenance (
    MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
    AssetID INT NOT NULL,
    IssueDescription NVARCHAR(MAX) NOT NULL,           -- Mô tả lỗi / hỏng hóc
    ReportedBy INT NULL,                               -- Người báo lỗi (EmployeeID)
    SentDate DATE NOT NULL,                            -- Ngày gửi sửa
    ExpectedReturnDate DATE NULL,                      -- Ngày dự kiến xong
    ActualReturnDate DATE NULL,                        -- Ngày nhận lại thực tế
    Cost DECIMAL(18,2) NULL DEFAULT 0,                 -- Chi phí sửa chữa
    VendorName NVARCHAR(255) NULL,                     -- Đơn vị sửa chữa
    Status NVARCHAR(30) NOT NULL DEFAULT 'In-Progress' 
        CHECK (Status IN ('In-Progress', 'Completed', 'Cannot-Repair')),
    ResultNote NVARCHAR(MAX) NULL,                     -- Kết quả sửa chữa
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Maintenance_Assets FOREIGN KEY (AssetID) 
        REFERENCES dbo.Assets(AssetID),
    CONSTRAINT FK_Maintenance_ReportedBy FOREIGN KEY (ReportedBy) 
        REFERENCES dbo.Employees(EmployeeID)
);
GO

-- =========================================================================================
-- 3. TẠO INDEXES TỐI ƯU HIỆU NĂNG TRUY VẤN
-- =========================================================================================
CREATE INDEX IX_Assets_Status ON dbo.Assets(Status);
CREATE INDEX IX_Assets_CategoryID ON dbo.Assets(CategoryID);
CREATE INDEX IX_Assets_CurrentHolderID ON dbo.Assets(CurrentHolderID);
CREATE INDEX IX_Employees_DepartmentID ON dbo.Employees(DepartmentID);
CREATE INDEX IX_Employees_Status ON dbo.Employees(Status);
CREATE INDEX IX_History_AssetID_ActionDate ON dbo.AssetHandoverHistory(AssetID, ActionDate DESC);
GO

-- =========================================================================================
-- 4. TẠO CÁC VIEWS THỐNG KÊ & BÁO CÁO (VIEWS FOR DASHBOARD & REPORTS)
-- =========================================================================================

-- View 4.1: Chi tiết Tài sản kèm Vị trí Động (Quy tắc: Có chủ -> Vị trí là Bộ phận; Không có chủ -> Vị trí là Kho)
CREATE OR ALTER VIEW dbo.vw_AssetsDetail
AS
SELECT 
    a.AssetID,
    a.AssetCode,
    a.AssetName,
    a.CategoryID,
    c.CategoryCode,
    c.CategoryName,
    a.Brand,
    a.MaterialCode,
    a.SerialNumber,
    a.PurchaseDate,
    a.WarrantyExpireDate,
    a.SupplierID,
    s.SupplierName,
    a.Status,
    a.CurrentHolderID,
    e.EmployeeCode AS HolderCode,
    e.FullName AS HolderName,
    d.DepartmentID,
    d.DepartmentName AS HolderDepartment,
    -- Vị trí động:
    CASE 
        WHEN a.CurrentHolderID IS NOT NULL THEN ISNULL(d.DepartmentName, N'Đã cấp phát')
        ELSE ISNULL(a.WarehouseLocation, N'Kho IT')
    END AS DynamicLocation,
    a.WarehouseLocation,
    a.Note,
    a.CreatedAt,
    a.UpdatedAt
FROM dbo.Assets a
INNER JOIN dbo.AssetCategories c ON a.CategoryID = c.CategoryID
LEFT JOIN dbo.Suppliers s ON a.SupplierID = s.SupplierID
LEFT JOIN dbo.Employees e ON a.CurrentHolderID = e.EmployeeID
LEFT JOIN dbo.Departments d ON e.DepartmentID = d.DepartmentID;
GO

-- View 4.2: Thống kê Tổng quan KPI (Tổng số, Đang dùng, Tồn kho, Hỏng hóc, Bảo trì)
CREATE OR ALTER VIEW dbo.vw_AssetKPIOverview
AS
SELECT 
    COUNT(*) AS TotalAssets,
    SUM(CASE WHEN Status = 'In-Use' THEN 1 ELSE 0 END) AS InUseCount,
    SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) AS AvailableCount,
    SUM(CASE WHEN Status = 'Broken' THEN 1 ELSE 0 END) AS BrokenCount,
    SUM(CASE WHEN Status = 'Maintenance' THEN 1 ELSE 0 END) AS MaintenanceCount,
    SUM(CASE WHEN Status = 'Disposed' THEN 1 ELSE 0 END) AS DisposedCount
FROM dbo.Assets;
GO

-- View 4.3: Cảnh báo Nhân viên Nghỉ việc còn giữ Máy hoặc Tài khoản chưa Khóa
CREATE OR ALTER VIEW dbo.vw_EmployeeLeaveAlerts
AS
SELECT 
    e.EmployeeID,
    e.EmployeeCode,
    e.FullName,
    d.DepartmentName,
    e.LeaveDate,
    e.QAD_Status,
    e.OA_Status,
    e.Email_Status,
    e.AD_Status,
    (SELECT COUNT(*) FROM dbo.Assets a WHERE a.CurrentHolderID = e.EmployeeID) AS HoldingAssetCount
FROM dbo.Employees e
LEFT JOIN dbo.Departments d ON e.DepartmentID = d.DepartmentID
WHERE e.LeaveDate IS NOT NULL 
  AND (
      e.QAD_Status != 'Disable' OR 
      e.OA_Status != 'Disable' OR 
      e.Email_Status != 'Disable' OR 
      e.AD_Status != 'Disable' OR 
      EXISTS (SELECT 1 FROM dbo.Assets a WHERE a.CurrentHolderID = e.EmployeeID)
  );
GO

-- =========================================================================================
-- 5. STORED PROCEDURES NGHIỆP VỤ (CẤP PHÁT & THU HỒI TỰ ĐỘNG GHI LOG)
-- =========================================================================================

-- SP 5.1: Cấp phát thiết bị cho nhân viên
CREATE OR ALTER PROCEDURE dbo.sp_AssignAsset
    @AssetID INT,
    @ToEmployeeID INT,
    @ConditionStatus NVARCHAR(255) = N'Bình thường',
    @Note NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100) = N'Admin'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @PrevHolderID INT, @PrevDeptID INT, @NewDeptID INT, @WarehouseLoc NVARCHAR(255);

        SELECT @PrevHolderID = CurrentHolderID, @WarehouseLoc = WarehouseLocation 
        FROM dbo.Assets WHERE AssetID = @AssetID;

        SELECT @NewDeptID = DepartmentID FROM dbo.Employees WHERE EmployeeID = @ToEmployeeID;

        -- 1. Cập nhật Asset
        UPDATE dbo.Assets
        SET 
            Status = 'In-Use',
            CurrentHolderID = @ToEmployeeID,
            UpdatedAt = GETDATE()
        WHERE AssetID = @AssetID;

        -- 2. Ghi lịch sử Handover
        INSERT INTO dbo.AssetHandoverHistory (
            AssetID, ActionType, FromEmployeeID, ToEmployeeID, 
            FromDepartmentID, ToDepartmentID, FromLocation, ToLocation, 
            ActionDate, ConditionStatus, Note, CreatedBy
        )
        VALUES (
            @AssetID, 'Assign', @PrevHolderID, @ToEmployeeID,
            NULL, @NewDeptID, @WarehouseLoc, (SELECT DepartmentName FROM dbo.Departments WHERE DepartmentID = @NewDeptID),
            GETDATE(), @ConditionStatus, @Note, @CreatedBy
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP 5.2: Thu hồi thiết bị về kho
CREATE OR ALTER PROCEDURE dbo.sp_ReturnAsset
    @AssetID INT,
    @WarehouseLocation NVARCHAR(255) = N'Kho IT - Kệ A1',
    @IsBroken BIT = 0,
    @ConditionStatus NVARCHAR(255) = N'Bình thường',
    @Note NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(100) = N'Admin'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @PrevHolderID INT, @PrevDeptID INT, @NewStatus NVARCHAR(30);

        SELECT @PrevHolderID = CurrentHolderID FROM dbo.Assets WHERE AssetID = @AssetID;
        IF @PrevHolderID IS NOT NULL
        BEGIN
            SELECT @PrevDeptID = DepartmentID FROM dbo.Employees WHERE EmployeeID = @PrevHolderID;
        END

        SET @NewStatus = CASE WHEN @IsBroken = 1 THEN 'Broken' ELSE 'Available' END;

        -- 1. Cập nhật Asset
        UPDATE dbo.Assets
        SET 
            Status = @NewStatus,
            CurrentHolderID = NULL,
            WarehouseLocation = @WarehouseLocation,
            UpdatedAt = GETDATE()
        WHERE AssetID = @AssetID;

        -- 2. Ghi log lịch sử Handover
        INSERT INTO dbo.AssetHandoverHistory (
            AssetID, ActionType, FromEmployeeID, ToEmployeeID, 
            FromDepartmentID, ToDepartmentID, FromLocation, ToLocation, 
            ActionDate, ConditionStatus, Note, CreatedBy
        )
        VALUES (
            @AssetID, 'Return', @PrevHolderID, NULL,
            @PrevDeptID, NULL, (SELECT DepartmentName FROM dbo.Departments WHERE DepartmentID = @PrevDeptID), @WarehouseLocation,
            GETDATE(), @ConditionStatus, @Note, @CreatedBy
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- =========================================================================================
-- 6. SEED DATA MẪU (THỰC TẾ & ĐẦY ĐỦ ĐỂ CHẠY NGAY)
-- =========================================================================================

-- 6.1 Thêm Bộ phận
INSERT INTO dbo.Departments (DepartmentCode, DepartmentName, Description) VALUES
(N'IT', N'Phòng Công Nghệ Thông Tin', N'Quản trị hệ thống và hạ tầng IT'),
(N'HR', N'Phòng Hành Chính Nhân Sự', N'Quản lý tuyển dụng, nhân sự và văn phòng phẩm'),
(N'ACC', N'Phòng Kế Toán Tài Chính', N'Quản lý thu chi và tài sản cố định'),
(N'SALES', N'Phòng Kinh Doanh & Marketing', N'Đội ngũ phát triển thị trường'),
(N'RND', N'Phòng Nghiên Cứu & Phát Triển', N'R&D Team');

-- 6.2 Thêm Nhà cung cấp
INSERT INTO dbo.Suppliers (SupplierCode, SupplierName, ContactPerson, Phone, Email, Address) VALUES
(N'SUP-DELL', N'Công ty CP Phân Phối Dell Việt Nam', N'Nguyễn Văn An', N'0901234567', N'sales@dellvn.com', N'Hà Nội'),
(N'SUP-FPT', N'FPT Information System', N'Trần Thị Bích', N'0912345678', N'contact@fis.com.vn', N'TP. Hồ Chí Minh'),
(N'SUP-PHONGVU', N'Công ty CP Thương mại Phong Vũ', N'Lê Hoàng Long', N'0988889999', N'b2b@phongvu.vn', N'Đà Nẵng');

-- 6.3 Thêm Phân loại tài sản
INSERT INTO dbo.AssetCategories (CategoryCode, CategoryName, Description) VALUES
(N'LAPTOP', N'Máy tính xách tay (Laptop)', N'Laptop làm việc cấp cho nhân viên'),
(N'PC', N'Máy tính để bàn (Desktop PC)', N'Máy tính bàn cố định tại văn phòng'),
(N'MONITOR', N'Màn hình hiển thị (Monitor)', N'Màn hình ngoài 24 - 27 inch'),
(N'NETWORK', N'Thiết bị mạng (Router/Switch/AP)', N'Hạ tầng mạng văn phòng'),
(N'PRINTER', N'Máy in & Thiết bị phụ trợ', N'Máy in laser, scan...');

-- 6.4 Thêm Nhân viên & Tình trạng tài khoản kèm theo
INSERT INTO dbo.Employees (EmployeeCode, FullName, DepartmentID, Title, Email, Phone, JoinDate, LeaveDate, QAD_Status, OA_Status, Email_Status, AD_Status, Status) VALUES
(N'EMP001', N'Trần Văn Khoa', 1, N'Trưởng phòng IT', N'khoa.tran@company.com', N'0909111222', '2022-01-15', NULL, 'Available', 'Available', 'Available', 'Available', 'Active'),
(N'EMP002', N'Lê Thị Mai', 2, N'Trưởng phòng HR', N'mai.le@company.com', N'0909333444', '2022-03-01', NULL, 'Available', 'Available', 'Available', 'Available', 'Active'),
(N'EMP003', N'Nguyễn Hoàng Dũng', 1, N'Kỹ sư Hệ thống (DevOps)', N'dung.nguyen@company.com', N'0911223344', '2023-06-10', NULL, 'Available', 'Available', 'Available', 'Available', 'Active'),
(N'EMP004', N'Phạm Thu Hà', 3, N'Chuyên viên Kế toán', N'ha.pham@company.com', N'0933445566', '2023-08-20', NULL, 'Available', 'Available', 'Available', 'Available', 'Active'),
(N'EMP005', N'Võ Minh Tuấn', 4, N'Nhân viên Kinh Doanh', N'tuan.vo@company.com', N'0977889900', '2023-11-01', '2026-08-01', 'Disable', 'Disable', 'Available', 'Disable', 'Resigned'); -- Đã nghỉ việc, còn Email chưa khóa

-- Cập nhật Manager cho Departments
UPDATE dbo.Departments SET ManagerID = 1 WHERE DepartmentCode = 'IT';
UPDATE dbo.Departments SET ManagerID = 2 WHERE DepartmentCode = 'HR';

-- 6.5 Thêm Danh mục Thiết bị
INSERT INTO dbo.Assets (AssetCode, AssetName, CategoryID, Brand, MaterialCode, SerialNumber, PurchaseDate, WarrantyExpireDate, SupplierID, Status, CurrentHolderID, WarehouseLocation, Note) VALUES
-- Đang cấp phát (In-Use)
(N'AST-LT-001', N'Dell Latitude 5420 i7/16GB/512GB', 1, N'Dell', N'VT-DELL-5420', N'SN-DLL-8891', '2023-01-10', '2026-01-10', 1, 'In-Use', 1, NULL, N'Máy Trưởng phòng IT'),
(N'AST-LT-002', N'ThinkPad T14s Gen 3 Ryzen 7', 1, N'Lenovo', N'VT-LNV-T14S', N'SN-LNV-3342', '2023-07-15', '2026-07-15', 2, 'In-Use', 3, NULL, N'Máy cấp cho DevOps'),
(N'AST-LT-003', N'Dell Vostro 3510 i5/8GB', 1, N'Dell', N'VT-DELL-3510', N'SN-DLL-1290', '2023-09-01', '2025-09-01', 3, 'In-Use', 4, NULL, N'Máy phòng Kế toán'),
(N'AST-MN-001', N'Màn hình Dell Ultrasharp U2422H 24"', 3, N'Dell', N'VT-MN-U2422', N'SN-MON-5561', '2023-02-20', '2026-02-20', 1, 'In-Use', 3, NULL, N'Màn hình phụ DevOps'),

-- Tồn kho sẵn sàng (Available)
(N'AST-LT-004', N'MacBook Air M2 16GB/256GB', 1, N'Apple', N'VT-APL-M2', N'SN-APL-9981', '2024-01-05', '2027-01-05', 2, 'Available', NULL, N'Kho IT - Tủ Kính A2', N'Sẵn sàng cấp cho Designer / PM mới'),
(N'AST-LT-005', N'Dell Latitude 3420 i5/8GB', 1, N'Dell', N'VT-DELL-3420', N'SN-DLL-7721', '2023-10-12', '2025-10-12', 1, 'Available', NULL, N'Kho IT - Kệ A1', N'Máy backup dự phòng'),
(N'AST-MN-002', N'Màn hình LG 27QN600 27" 2K', 3, N'LG', N'VT-MN-LG27', N'SN-LG-1123', '2024-02-15', '2026-02-15', 3, 'Available', NULL, N'Kho IT - Kệ Màn Hình B1', N'Màn hình sẵn sàng'),

-- Bị hỏng (Broken)
(N'AST-LT-006', N'HP ProBook 450 G8 (Lỗi màn hình sọc)', 1, N'HP', N'VT-HP-450G8', N'SN-HP-9901', '2022-05-10', '2024-05-10', 3, 'Broken', NULL, N'Kho IT - Kệ Chờ Sửa', N'Màn hình bị va đập sọc xanh, chờ duyệt chi phí sửa'),

-- Đang bảo hành (Maintenance)
(N'AST-PR-001', N'Máy in HP LaserJet Pro M404dn', 5, N'HP', N'VT-PR-M404', N'SN-PRT-4432', '2023-03-01', '2025-03-01', 2, 'Maintenance', NULL, N'Trung tâm bảo hành FPT', N'Lỗi kẹt giấy liên tục, đã gửi FPT bảo hành ngày 15/08/2026');

-- 6.6 Thêm Lịch sử Cấp phát mẫu
INSERT INTO dbo.AssetHandoverHistory (AssetID, ActionType, FromEmployeeID, ToEmployeeID, FromDepartmentID, ToDepartmentID, FromLocation, ToLocation, ActionDate, ConditionStatus, Note, CreatedBy) VALUES
(1, 'Assign', NULL, 1, NULL, 1, N'Kho IT', N'Phòng Công Nghệ Thông Tin', '2023-01-15 09:00:00', N'Mới 100%, nguyên hộp', N'Cấp máy mới cho Trưởng phòng IT', N'Admin'),
(2, 'Assign', NULL, 3, NULL, 1, N'Kho IT', N'Phòng Công Nghệ Thông Tin', '2023-07-16 10:30:00', N'Mới 100%', N'Cấp laptop làm việc cho Kỹ sư DevOps', N'Admin'),
(3, 'Assign', NULL, 4, NULL, 3, N'Kho IT', N'Phòng Kế Toán Tài Chính', '2023-09-05 14:00:00', N'Máy đã qua sử dụng, hoạt động tốt', N'Cấp cho kế toán mới', N'Admin');

-- 6.7 Thêm Bản ghi Bảo trì mẫu
INSERT INTO dbo.AssetMaintenance (AssetID, IssueDescription, ReportedBy, SentDate, ExpectedReturnDate, Cost, VendorName, Status, ResultNote) VALUES
(8, N'Lỗi cụm sấy và kẹt giấy liên tục khi in 2 mặt', 1, '2026-08-15', '2026-08-25', 550000, N'FPT Services', 'In-Progress', N'Đang chờ linh kiện thay thế');
GO

PRINT N'=== TRIỂN KHAI CƠ SỞ DỮ LIỆU QLKhoAssetDB THÀNH CÔNG ===';
GO
