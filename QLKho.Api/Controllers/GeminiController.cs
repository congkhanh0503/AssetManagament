using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace QLKho.Api.Controllers;

[ApiController]
[Route("api/gemini")]
public class GeminiController : ControllerBase
{
    private static readonly string[] Models = ["gemini-3.5-flash-lite", "gemini-flash-latest"];
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GeminiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("generate")]
    [EnableRateLimiting("Gemini")]
    [RequestSizeLimit(512 * 1024)]
    public async Task<IActionResult> Generate([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Chưa cấu hình Gemini:ApiKey trên máy chủ."
            });
        }

        if (payload.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty("contents", out var contents) ||
            contents.ValueKind != JsonValueKind.Array ||
            contents.GetArrayLength() == 0)
        {
            return BadRequest(new { message = "Nội dung yêu cầu Gemini không hợp lệ." });
        }

        var client = _httpClientFactory.CreateClient();
        string? lastError = null;

        foreach (var model in Models)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent");
            request.Headers.Add("x-goog-api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json");

            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return Content(responseBody, "application/json", Encoding.UTF8);
                }

                lastError = GetProviderError(responseBody) ?? $"Lỗi API Gemini ({(int)response.StatusCode}).";
            }
            catch (HttpRequestException)
            {
                lastError = "Không thể kết nối đến máy chủ Gemini.";
            }
        }

        return StatusCode(StatusCodes.Status502BadGateway, new
        {
            message = lastError ?? "Không thể kết nối đến Gemini AI."
        });
    }

    private static string? GetProviderError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
