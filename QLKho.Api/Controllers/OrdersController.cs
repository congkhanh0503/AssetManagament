using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? keyword)
    {
        var orders = await _orderService.GetAllOrdersAsync(status, keyword);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _orderService.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderID }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _orderService.UpdateOrderAsync(id, dto);
        if (updated == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _orderService.DeleteOrderAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy đơn hàng" });
        return Ok(new { message = "Xóa đơn hàng thành công" });
    }

    [HttpPost("{id}/devices")]
    public async Task<IActionResult> AddDevice(int id, [FromBody] AddDeviceToOrderDto dto)
    {
        try
        {
            var device = await _orderService.AddDeviceToOrderAsync(id, dto);
            return Ok(device);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/devices/bulk")]
    public async Task<IActionResult> BulkAddDevices(int id, [FromBody] BulkAddDevicesToOrderDto dto)
    {
        try
        {
            var devices = await _orderService.BulkAddDevicesToOrderAsync(id, dto);
            return Ok(devices);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/devices/{deviceId}")]
    public async Task<IActionResult> RemoveDevice(int id, int deviceId)
    {
        var success = await _orderService.RemoveDeviceFromOrderAsync(id, deviceId);
        if (!success) return NotFound(new { message = "Không tìm thấy thiết bị cần xóa" });
        return Ok(new { message = "Xóa thiết bị khỏi đơn hàng thành công" });
    }

    [HttpPost("{id}/sync-assets")]
    public async Task<IActionResult> SyncToAssets(int id)
    {
        try
        {
            var result = await _orderService.SyncOrderToAssetsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
