using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu." });
        }

        var username = dto.Username.Trim().ToLower();
        var password = dto.Password.Trim();

        // Tìm user trong database
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username && u.IsActive);

        // Kiểm tra mật khẩu (hỗ trợ tài khoản admin: 123456)
        if (user == null || user.Password != password)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        // Cập nhật thời điểm đăng nhập gần nhất
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Tạo token phiên làm việc đơn giản / an toàn
        var sessionToken = $"token_{user.UserID}_{Guid.NewGuid():N}";

        return Ok(new LoginResponseDto
        {
            Token = sessionToken,
            UserID = user.UserID,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Avatar = user.Avatar,
            Email = user.Email
        });
    }

    // GET: api/auth/me
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetCurrentUser([FromQuery] string? username = "admin")
    {
        var targetUsername = (username ?? "admin").Trim().ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == targetUsername && u.IsActive);

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy phiên làm việc người dùng." });
        }

        return Ok(new
        {
            user.UserID,
            user.Username,
            user.FullName,
            user.Role,
            user.Avatar,
            user.Email,
            user.LastLoginAt
        });
    }

    // POST: api/auth/change-password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, [FromQuery] string username = "admin")
    {
        var targetUsername = username.Trim().ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == targetUsername && u.IsActive);

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng." });
        }

        if (user.Password != dto.CurrentPassword.Trim())
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
        }

        user.Password = dto.NewPassword.Trim();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công!" });
    }
}
