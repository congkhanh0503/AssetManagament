using Microsoft.AspNetCore.Mvc;
using QLKho.Api.Controllers;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using Xunit;

namespace QLKho.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ValidAdminCredentials_ReturnsSuccessAndToken()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(Login_ValidAdminCredentials_ReturnsSuccessAndToken));
        var controller = new AuthController(context);

        context.Users.Add(new User
        {
            Username = "admin",
            Password = "123456",
            FullName = "Quản Trị Viên (Admin)",
            Role = "Admin",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var dto = new LoginRequestDto
        {
            Username = "admin",
            Password = "123456"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDto>(okResult.Value);

        Assert.NotNull(response.Token);
        Assert.Equal("admin", response.Username);
        Assert.Equal("Admin", response.Role);
        Assert.Equal("Quản Trị Viên (Admin)", response.FullName);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(Login_InvalidPassword_ReturnsUnauthorized));
        var controller = new AuthController(context);

        context.Users.Add(new User
        {
            Username = "admin",
            Password = "123456",
            FullName = "Admin",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var dto = new LoginRequestDto
        {
            Username = "admin",
            Password = "wrong_password_999"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(Login_NonExistentUser_ReturnsUnauthorized));
        var controller = new AuthController(context);

        var dto = new LoginRequestDto
        {
            Username = "unknown_user",
            Password = "123456"
        };

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task ChangePassword_ValidCurrentPassword_ChangesPasswordSuccessfully()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ChangePassword_ValidCurrentPassword_ChangesPasswordSuccessfully));
        var controller = new AuthController(context);

        var user = new User
        {
            Username = "admin",
            Password = "123456",
            FullName = "Admin",
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = "new_secret_password_2026"
        };

        // Act
        var result = await controller.ChangePassword(dto, "admin");

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updatedUser = await context.Users.FindAsync(user.UserID);
        Assert.NotNull(updatedUser);
        Assert.Equal("new_secret_password_2026", updatedUser.Password);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var context = TestDbHelper.GetInMemoryDbContext(nameof(ChangePassword_WrongCurrentPassword_ReturnsBadRequest));
        var controller = new AuthController(context);

        context.Users.Add(new User
        {
            Username = "admin",
            Password = "123456",
            FullName = "Admin",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "wrong_old_pwd",
            NewPassword = "new_secret_password_2026"
        };

        // Act
        var result = await controller.ChangePassword(dto, "admin");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
