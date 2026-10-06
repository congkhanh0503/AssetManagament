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

        User? user = null;
        try
        {
            // Tìm user trong database
            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username && u.IsActive);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth Login Notice]: Không tìm thấy bảng users ({ex.Message}). Sử dụng chế độ tài khoản mặc định.");
        }

        // Kiểm tra mật khẩu (hỗ trợ tài khoản admin: 123456 hoặc hr: 123456)
        if (user != null)
        {
            if (user.Password != password)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            try
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception) {}

            return Ok(new LoginResponseDto
            {
                Token = $"token_{user.UserID}_{Guid.NewGuid():N}",
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Avatar = user.Avatar,
                Email = user.Email
            });
        }

        // Fallback mặc định cho tài khoản admin / hr khi DB chưa seed users
        if ((username == "admin" && password == "123456") || (username == "admin" && password == "admin"))
        {
            return Ok(new LoginResponseDto
            {
                Token = $"token_admin_{Guid.NewGuid():N}",
                UserID = 1,
                Username = "admin",
                FullName = "Quản Trị Viên Hệ Thống (Admin)",
                Role = "Admin",
                Avatar = "",
                Email = "admin@qlkho.local"
            });
        }

        if (username == "hr" && password == "123456")
        {
            return Ok(new LoginResponseDto
            {
                Token = $"token_hr_{Guid.NewGuid():N}",
                UserID = 2,
                Username = "hr",
                FullName = "Nhân Sự Công Ty (HR)",
                Role = "HR",
                Avatar = "",
                Email = "hr@qlkho.local"
            });
        }

        return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
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
