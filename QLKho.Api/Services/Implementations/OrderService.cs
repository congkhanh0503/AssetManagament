using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderSummaryDto>> GetAllOrdersAsync(string? status = null, string? keyword = null)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Devices)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim().ToLower();
            query = query.Where(o => o.Status.ToLower() == s);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            query = query.Where(o =>
                o.OrderCode.ToLower().Contains(k) ||
                o.OrderName.ToLower().Contains(k) ||
                (o.ProjectName != null && o.ProjectName.ToLower().Contains(k)) ||
                (o.SupplierName != null && o.SupplierName.ToLower().Contains(k))
            );
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToSummaryDto).ToList();
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Category)
            .Include(o => o.Devices)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderID == orderId);

        if (order == null) return null;

        var dto = new OrderDetailDto
        {
            OrderID = order.OrderID,
            OrderCode = order.OrderCode,
            OrderName = order.OrderName,
            IsProjectBased = order.IsProjectBased,
            ProjectName = order.ProjectName,
            SupplierID = order.SupplierID,
            SupplierName = order.SupplierName,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            ActualDeliveryDate = order.ActualDeliveryDate,
            Status = order.Status,
            Note = order.Note,
            CreatedBy = order.CreatedBy,
            TotalExpectedQuantity = order.Items.Sum(i => i.ExpectedQuantity),
            TotalReceivedQuantity = order.Devices.Count,
            ItemTypesCount = order.Items.Count,
            TransferredAssetsCount = order.Devices.Count(d => d.IsTransferredToAsset),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemDetailDto
            {
                OrderItemID = i.OrderItemID,
                OrderID = i.OrderID,
                CategoryID = i.CategoryID,
                CategoryName = i.Category?.CategoryName ?? i.CategoryName,
                ModelName = i.ModelName,
                Brand = i.Brand,
                Specifications = i.Specifications,
                ExpectedQuantity = i.ExpectedQuantity,
                ReceivedQuantity = order.Devices.Count(d => d.OrderItemID == i.OrderItemID),
                UnitPrice = i.UnitPrice,
                Note = i.Note
            }).ToList(),
            Devices = order.Devices.OrderBy(d => d.DeviceItemID).Select(d => new OrderDeviceItemDto
            {
                DeviceItemID = d.DeviceItemID,
                OrderID = d.OrderID,
                OrderItemID = d.OrderItemID,
                AssetCode = d.AssetCode,
                SerialNumber = d.SerialNumber,
                AssetName = d.AssetName,
                CategoryID = d.CategoryID,
                Brand = d.Brand,
                Specifications = d.Specifications,
                WarehouseLocation = d.WarehouseLocation,
                IsTransferredToAsset = d.IsTransferredToAsset,
                AssetID = d.AssetID,
                CreatedAt = d.CreatedAt
            }).ToList()
        };

        return dto;
    }

    public async Task<OrderDetailDto> CreateOrderAsync(CreateOrderDto dto, string? createdBy = "Admin")
    {
        // 1. Tự sinh mã đơn hàng nếu người dùng không nhập
        string code = dto.OrderCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            string datePrefix = DateTime.Now.ToString("yyyyMMdd");
            int todayCount = await _context.Orders.CountAsync(o => o.OrderCode.StartsWith($"PO-{datePrefix}"));
            code = $"PO-{datePrefix}-{(todayCount + 1):D3}";
        }

        // Kiểm tra trùng mã đơn
        bool isDuplicate = await _context.Orders.AnyAsync(o => o.OrderCode.ToLower() == code.ToLower());
        if (isDuplicate)
        {
            code = $"{code}-{DateTime.Now:HHmmss}";
        }

        // Lấy tên nhà cung cấp nếu có SupplierID
        string? supplierName = dto.SupplierName;
        if (dto.SupplierID.HasValue && string.IsNullOrWhiteSpace(supplierName))
        {
            var supplier = await _context.Suppliers.FindAsync(dto.SupplierID.Value);
            supplierName = supplier?.SupplierName;
        }

        var order = new Order
        {
            OrderCode = code,
            OrderName = dto.OrderName.Trim(),
            IsProjectBased = dto.IsProjectBased,
            ProjectName = dto.IsProjectBased ? dto.ProjectName?.Trim() : null,
            SupplierID = dto.SupplierID,
            SupplierName = supplierName,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            Status = "Pending",
            Note = dto.Note?.Trim(),
            CreatedBy = createdBy ?? "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 2. Thêm danh sách dòng loại thiết bị
        if (dto.Items != null && dto.Items.Any())
        {
            foreach (var itemDto in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(itemDto.ModelName)) continue;

                string? catName = itemDto.CategoryName;
                if (itemDto.CategoryID.HasValue && string.IsNullOrWhiteSpace(catName))
                {
                    var cat = await _context.AssetCategories.FindAsync(itemDto.CategoryID.Value);
                    catName = cat?.CategoryName;
                }

                order.Items.Add(new OrderItem
                {
                    CategoryID = itemDto.CategoryID,
                    CategoryName = catName,
                    ModelName = itemDto.ModelName.Trim(),
                    Brand = itemDto.Brand?.Trim(),
                    Specifications = itemDto.Specifications?.Trim(),
                    ExpectedQuantity = Math.Max(1, itemDto.ExpectedQuantity),
                    ReceivedQuantity = 0,
                    UnitPrice = itemDto.UnitPrice,
                    Note = itemDto.Note?.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetOrderByIdAsync(order.OrderID))!;
    }

    public async Task<OrderDetailDto?> UpdateOrderAsync(int orderId, UpdateOrderDto dto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return null;

        order.OrderName = dto.OrderName.Trim();
        order.IsProjectBased = dto.IsProjectBased;
        order.ProjectName = dto.IsProjectBased ? dto.ProjectName?.Trim() : null;
        order.SupplierID = dto.SupplierID;
        order.SupplierName = dto.SupplierName;
        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            order.Status = dto.Status;
        }
        order.Note = dto.Note?.Trim();
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(orderId);
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Devices)
            .FirstOrDefaultAsync(o => o.OrderID == orderId);

        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OrderDeviceItemDto> AddDeviceToOrderAsync(int orderId, AddDeviceToOrderDto dto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var item = await _context.OrderItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.OrderItemID == dto.OrderItemID && i.OrderID == orderId);
        if (item == null) throw new KeyNotFoundException("Dòng thiết bị không thuộc đơn hàng này.");

        // Sinh mã tài sản tự động nếu chưa có
        string assetCode = dto.AssetCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(assetCode))
        {
            assetCode = await GenerateAssetCodeAsync(item.CategoryID, item.ModelName);
        }

        string assetName = dto.AssetName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(assetName))
        {
            assetName = item.ModelName;
        }

        var device = new OrderDeviceItem
        {
            OrderID = orderId,
            OrderItemID = item.OrderItemID,
            AssetCode = assetCode,
            SerialNumber = dto.SerialNumber?.Trim(),
            AssetName = assetName,
            CategoryID = item.CategoryID,
            Brand = item.Brand,
            Specifications = !string.IsNullOrWhiteSpace(dto.Specifications) ? dto.Specifications.Trim() : item.Specifications,
            WarehouseLocation = dto.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1",
            IsTransferredToAsset = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderDeviceItems.Add(device);

        // Cập nhật số lượng đã nhận của OrderItem
        item.ReceivedQuantity = await _context.OrderDeviceItems.CountAsync(d => d.OrderItemID == item.OrderItemID) + 1;

        // Cập nhật trạng thái Order sang Receiving nếu đang Pending
        if (order.Status == "Pending")
        {
            order.Status = "Receiving";
        }
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OrderDeviceItemDto
        {
            DeviceItemID = device.DeviceItemID,
            OrderID = device.OrderID,
            OrderItemID = device.OrderItemID,
            AssetCode = device.AssetCode,
            SerialNumber = device.SerialNumber,
            AssetName = device.AssetName,
            CategoryID = device.CategoryID,
            CategoryName = item.Category?.CategoryName ?? item.CategoryName,
            Brand = device.Brand,
            Specifications = device.Specifications,
            WarehouseLocation = device.WarehouseLocation,
            IsTransferredToAsset = device.IsTransferredToAsset,
            AssetID = device.AssetID,
            CreatedAt = device.CreatedAt
        };
    }

    public async Task<List<OrderDeviceItemDto>> BulkAddDevicesToOrderAsync(int orderId, BulkAddDevicesToOrderDto dto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var item = await _context.OrderItems
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.OrderItemID == dto.OrderItemID && i.OrderID == orderId);
        if (item == null) throw new KeyNotFoundException("Dòng thiết bị không thuộc đơn hàng này.");

        var result = new List<OrderDeviceItemDto>();
        var cleanSerials = dto.SerialNumbers
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct()
            .ToList();

        if (!cleanSerials.Any()) return result;

        int currentMaxNum = await GetMaxAssetCodeNumberAsync(dto.AssetCodePrefix ?? GetPrefixByCategory(item.CategoryID, item.ModelName));

        foreach (var sn in cleanSerials)
        {
            currentMaxNum++;
            string prefix = dto.AssetCodePrefix ?? GetPrefixByCategory(item.CategoryID, item.ModelName);
            string code = $"{prefix}{currentMaxNum:D5}";

            var device = new OrderDeviceItem
            {
                OrderID = orderId,
                OrderItemID = item.OrderItemID,
                AssetCode = code,
                SerialNumber = sn,
                AssetName = item.ModelName,
                CategoryID = item.CategoryID,
                Brand = item.Brand,
                Specifications = item.Specifications,
                WarehouseLocation = dto.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1",
                IsTransferredToAsset = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderDeviceItems.Add(device);
        }

        await _context.SaveChangesAsync();

        // Cập nhật lại số lượng đã nhận
        item.ReceivedQuantity = await _context.OrderDeviceItems.CountAsync(d => d.OrderItemID == item.OrderItemID);

        if (order.Status == "Pending")
        {
            order.Status = "Receiving";
        }
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var addedDevices = await _context.OrderDeviceItems
            .Where(d => d.OrderID == orderId && d.OrderItemID == item.OrderItemID)
            .OrderByDescending(d => d.DeviceItemID)
            .Take(cleanSerials.Count)
            .ToListAsync();

        return addedDevices.Select(d => new OrderDeviceItemDto
        {
            DeviceItemID = d.DeviceItemID,
            OrderID = d.OrderID,
            OrderItemID = d.OrderItemID,
            AssetCode = d.AssetCode,
            SerialNumber = d.SerialNumber,
            AssetName = d.AssetName,
            CategoryID = d.CategoryID,
            CategoryName = item.Category?.CategoryName ?? item.CategoryName,
            Brand = d.Brand,
            Specifications = d.Specifications,
            WarehouseLocation = d.WarehouseLocation,
            IsTransferredToAsset = d.IsTransferredToAsset,
            AssetID = d.AssetID,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task<bool> RemoveDeviceFromOrderAsync(int orderId, int deviceItemId)
    {
        var device = await _context.OrderDeviceItems
            .FirstOrDefaultAsync(d => d.OrderID == orderId && d.DeviceItemID == deviceItemId);

        if (device == null) return false;

        var item = await _context.OrderItems.FindAsync(device.OrderItemID);

        _context.OrderDeviceItems.Remove(device);
        await _context.SaveChangesAsync();

        if (item != null)
        {
            item.ReceivedQuantity = await _context.OrderDeviceItems.CountAsync(d => d.OrderItemID == item.OrderItemID);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<SyncOrderToAssetsResultDto> SyncOrderToAssetsAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Devices)
            .FirstOrDefaultAsync(o => o.OrderID == orderId);

        if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var pendingDevices = order.Devices.Where(d => !d.IsTransferredToAsset).ToList();
        if (!pendingDevices.Any())
        {
            return new SyncOrderToAssetsResultDto
            {
                Success = true,
                Message = "Tất cả thiết bị trong đơn hàng này đã được đồng bộ vào Quản lý tài sản từ trước.",
                TransferredCount = 0,
                TotalDevicesCount = order.Devices.Count,
                MissingQuantity = Math.Max(0, order.Items.Sum(i => i.ExpectedQuantity) - order.Devices.Count),
                OrderStatus = order.Status
            };
        }

        // Lấy danh mục mặc định đề phòng
        var defaultCat = await _context.AssetCategories.FirstOrDefaultAsync();
        int defaultCatId = defaultCat?.CategoryID ?? 1;

        int transferredCount = 0;
        foreach (var device in pendingDevices)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.OrderItemID == device.OrderItemID);

            // Đảm bảo không trùng mã AssetCode trong bảng assets
            string assetCode = device.AssetCode;
            int counter = 1;
            while (await _context.Assets.AnyAsync(a => a.AssetCode == assetCode))
            {
                assetCode = $"{device.AssetCode}_{counter++}";
            }

            var asset = new Asset
            {
                AssetCode = assetCode,
                AssetName = device.AssetName,
                CategoryID = device.CategoryID ?? orderItem?.CategoryID ?? defaultCatId,
                Brand = device.Brand ?? orderItem?.Brand,
                Specifications = device.Specifications ?? orderItem?.Specifications,
                SerialNumber = device.SerialNumber,
                PurchaseDate = order.OrderDate,
                SupplierID = order.SupplierID,
                Status = "Available",
                WarehouseLocation = device.WarehouseLocation ?? "Kho IT - Kệ A1",
                OrderID = order.OrderID,
                OrderCode = order.OrderCode,
                ProjectName = order.IsProjectBased ? order.ProjectName : null,
                Note = $"[Đơn hàng {order.OrderCode}] Nhập kho từ đơn hàng '{order.OrderName}'{(order.IsProjectBased ? $" (Dự án: {order.ProjectName})" : "")}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            device.IsTransferredToAsset = true;
            device.AssetID = asset.AssetID;
            transferredCount++;
        }

        // Tính toán trạng thái đơn hàng
        int totalExpected = order.Items.Sum(i => i.ExpectedQuantity);
        int totalReceived = order.Devices.Count;
        int missing = Math.Max(0, totalExpected - totalReceived);

        if (totalReceived >= totalExpected)
        {
            order.Status = "Completed"; // Đã hoàn tất đủ 100%
        }
        else
        {
            order.Status = "Partial"; // Nhận một phần (có thiết bị bị thiếu)
        }

        order.ActualDeliveryDate = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        string msg = missing > 0
            ? $"Đã đồng bộ {transferredCount} thiết bị sang tab Quản lý tài sản! ⚠️ Đơn hàng ghi nhận còn thiếu {missing} thiết bị do vận chuyển thiếu."
            : $"Đã đồng bộ thành công đầy đủ {transferredCount} thiết bị sang tab Quản lý tài sản!";

        return new SyncOrderToAssetsResultDto
        {
            Success = true,
            Message = msg,
            TransferredCount = transferredCount,
            TotalDevicesCount = totalReceived,
            MissingQuantity = missing,
            OrderStatus = order.Status
        };
    }

    #region Helpers
    private static OrderSummaryDto MapToSummaryDto(Order order)
    {
        int totalExpected = order.Items.Sum(i => i.ExpectedQuantity);
        int totalReceived = order.Devices.Count;

        return new OrderSummaryDto
        {
            OrderID = order.OrderID,
            OrderCode = order.OrderCode,
            OrderName = order.OrderName,
            IsProjectBased = order.IsProjectBased,
            ProjectName = order.ProjectName,
            SupplierID = order.SupplierID,
            SupplierName = order.SupplierName,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            ActualDeliveryDate = order.ActualDeliveryDate,
            Status = order.Status,
            Note = order.Note,
            CreatedBy = order.CreatedBy,
            TotalExpectedQuantity = totalExpected,
            TotalReceivedQuantity = totalReceived,
            ItemTypesCount = order.Items.Count,
            TransferredAssetsCount = order.Devices.Count(d => d.IsTransferredToAsset),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private static string GetPrefixByCategory(int? categoryId, string modelName)
    {
        string lower = modelName.ToLower();
        if (lower.Contains("laptop") || lower.Contains("notebook") || lower.Contains("elitebook") || lower.Contains("thinkpad"))
            return "TTH-NB";
        if (lower.Contains("desktop") || lower.Contains("pc") || lower.Contains("máy bàn"))
            return "TTH-PC";
        if (lower.Contains("màn hình") || lower.Contains("monitor") || lower.Contains("display"))
            return "TTH-MON";
        if (lower.Contains("chuột") || lower.Contains("mouse"))
            return "MOU-";
        if (lower.Contains("bàn phím") || lower.Contains("keyboard"))
            return "KB-";
        if (lower.Contains("máy in") || lower.Contains("printer"))
            return "PRI-";
        return "AST-";
    }

    private async Task<string> GenerateAssetCodeAsync(int? categoryId, string modelName)
    {
        string prefix = GetPrefixByCategory(categoryId, modelName);
        int maxNum = await GetMaxAssetCodeNumberAsync(prefix);
        return $"{prefix}{(maxNum + 1):D5}";
    }

    private async Task<int> GetMaxAssetCodeNumberAsync(string prefix)
    {
        var existingCodes = await _context.Assets
            .Where(a => a.AssetCode.StartsWith(prefix))
            .Select(a => a.AssetCode)
            .ToListAsync();

        var orderCodes = await _context.OrderDeviceItems
            .Where(d => d.AssetCode.StartsWith(prefix))
            .Select(d => d.AssetCode)
            .ToListAsync();

        int max = 0;
        foreach (var code in existingCodes.Concat(orderCodes))
        {
            var sub = code.Substring(prefix.Length);
            // Lấy các chữ số đầu tiên
            var numStr = new string(sub.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(numStr, out int num) && num > max)
            {
                max = num;
            }
        }
        return max;
    }
    #endregion
}
