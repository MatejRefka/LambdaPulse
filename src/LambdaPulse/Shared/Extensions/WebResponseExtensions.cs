using LambdaPulse.Engine.Features.State.Sessions;
using LambdaPulse.Engine.Http.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LambdaPulse.Engine.Shared.Extensions;

public static class WebResponseExtensions
{
    private static readonly HashSet<string> EngineCookieNames = new(StringComparer.Ordinal)
    {
        SessionConstants.SessionCookieName,
        SessionConstants.PreSessionCookieName,
        SessionConstants.AnonymousSessionCookieName
    };

    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower)
        }
    };

    public static async Task WriteBytesToBody(this WebResponse webResponse, byte[] bytes, CancellationToken cancellationToken = default)
    {
        webResponse.HasBody = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    public static async Task WriteStringToBody(this WebResponse webResponse, string text, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(text);

        if (!webResponse.Headers.ContainsKey("Content-Type"))
        {
            webResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        }

        webResponse.HasBody = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    public static async Task WriteJsonToBody<T>(this WebResponse webResponse, T anonymousObject, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(anonymousObject, CamelCase);
        var bytes = Encoding.UTF8.GetBytes(json);

        webResponse.Headers["Content-Type"] = "application/json";

        webResponse.HasBody = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    public static void ClearResponse(this WebResponse webResponse)
    {
        webResponse.StatusCode = null;
        webResponse.ResponsePhrase = null;
        webResponse.Headers.Clear();
        webResponse.Cookies.RemoveAll(cookie => !IsEngineCookie(cookie));
        webResponse.Body.SetLength(0);
        webResponse.Body.Position = 0;
        webResponse.HasBody = false;
    }

    public static void ApplyVaryHeader(this WebResponse webResponse, string headerName)
    {
        webResponse.Headers.TryGetValue("Vary", out var varyHeaderValue);
        if (string.IsNullOrWhiteSpace(varyHeaderValue))
        {
            webResponse.Headers["Vary"] = headerName;
        }
        else if (!varyHeaderValue.Contains(headerName, StringComparison.OrdinalIgnoreCase))
        {
            webResponse.Headers["Vary"] = $"{varyHeaderValue}, {headerName}";
        }
    }

    private static bool IsEngineCookie(string cookie)
    {
        if (string.IsNullOrWhiteSpace(cookie))
        {
            return false;
        }

        var separatorIndex = cookie.IndexOf('=');
        if (separatorIndex <= 0)
        {
            return false;
        }

        var cookieName = cookie[..separatorIndex].Trim();
        return EngineCookieNames.Contains(cookieName);
    }
}
