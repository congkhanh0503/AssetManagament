using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.Entities;

namespace QLKho.Api.Controllers;

public class ManagerRequestDto
{
    public int? EmployeeID { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ManagersController : ControllerBase
{
    private readonly AppDbContext _context;

    public ManagersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentManager>>> GetAll()
    {
        var list = await _context.DepartmentManagers.AsNoTracking().OrderBy(m => m.FullName).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentManager>> Create([FromBody] ManagerRequestDto dto)
    {
        var name = dto.FullName?.Trim();
        var email = dto.Email?.Trim();

        if (dto.EmployeeID.HasValue)
        {
            var emp = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == dto.EmployeeID.Value);
            if (emp == null) return BadRequest(new { message = "Không tìm thấy nhân sự." });
            name = emp.FullName;
            email = emp.Email;
            if (await _context.DepartmentManagers.AnyAsync(m => m.EmployeeID == emp.EmployeeID))
                return BadRequest(new { message = $"\"{emp.FullName}\" đã có trong danh sách Manager." });
        }

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Vui lòng chọn nhân sự hoặc nhập tên manager." });

        var m = new DepartmentManager { EmployeeID = dto.EmployeeID, FullName = name, Email = string.IsNullOrWhiteSpace(email) ? null : email };
        _context.DepartmentManagers.Add(m);
        await _context.SaveChangesAsync();
        return Ok(m);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _context.DepartmentManagers.FindAsync(id);
        if (m == null) return NotFound(new { message = "Không tìm thấy manager." });
        _context.DepartmentManagers.Remove(m);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
