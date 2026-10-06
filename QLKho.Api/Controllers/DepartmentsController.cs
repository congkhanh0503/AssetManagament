using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        try
        {
            var depts = await _departmentService.GetAllAsync();
            return Ok(depts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetDepartments Error]: {ex.Message} -> {ex.InnerException?.Message}");
            return StatusCode(500, new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        try
        {
            var dept = await _departmentService.GetByIdAsync(id);
            if (dept == null) return NotFound(new { message = "Không tìm thấy phòng ban" });
            return Ok(dept);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetDepartment Error]: {ex.Message} -> {ex.InnerException?.Message}");
            return StatusCode(500, new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetDepartment), new { id = created.DepartmentID }, created);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateDepartment Error]: {ex.Message} -> {ex.InnerException?.Message}");
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _departmentService.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy phòng ban" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateDepartment Error]: {ex.Message} -> {ex.InnerException?.Message}");
            return BadRequest(new { message = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            var success = await _departmentService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy phòng ban" });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
