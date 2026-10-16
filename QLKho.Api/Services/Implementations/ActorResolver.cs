using Microsoft.AspNetCore.Http;

namespace QLKho.Api.Services.Implementations;

/// <summary>
/// Lấy tên người thực hiện thao tác từ header X-Username do frontend gắn vào mỗi request.
/// Không có header (import, test, gọi trực tiếp) thì dùng giá trị dự phòng.
/// </summary>
internal static class ActorResolver
{
    public const string HeaderName = "X-Username";

    public static string Resolve(IHttpContextAccessor? accessor, string? fallback)
    {
        var raw = accessor?.HttpContext?.Request.Headers[HeaderName].ToString();
        var name = string.IsNullOrWhiteSpace(raw) ? null : Uri.UnescapeDataString(raw.Trim());
        if (string.IsNullOrWhiteSpace(name)) return fallback ?? "Admin";
        return name.Length > 100 ? name[..100] : name;
    }
}
