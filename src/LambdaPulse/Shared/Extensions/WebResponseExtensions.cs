using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LambdaPulse.Features.State.Sessions;
using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Shared.Extensions;

/// <summary>
/// Extension methods for the WebResponse class.
/// </summary>
public static class WebResponseExtensions
{
    /// <summary>
    /// Engine cookies that should not be removed from the response.
    /// </summary>
    public static readonly HashSet<string> EngineCookies = new(StringComparer.Ordinal)
    {
        SessionConstants.SessionCookieName,
        SessionConstants.PreSessionCookieName,
        SessionConstants.AnonymousSessionCookieName
    };

    /// <summary>
    /// CamelCase JSON options with KebabCaseLower enum converter for serialization.
    /// </summary>
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower)
        }
    };

    /// <summary>
    /// Writes a given byte array to the body of the WebResponse.
    /// </summary>
    public static async Task WriteBytesToBody(this WebResponse webResponse, byte[] bytes, CancellationToken cancellationToken = default)
    {
        webResponse.HasBody = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    /// <summary>
    /// Writes a given string to the body of the WebResponse.
    /// </summary>
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

    /// <summary>
    /// Writes a given anonymous object as JSON to the body of the WebResponse.
    /// </summary>
    public static async Task WriteJsonToBody<T>(this WebResponse webResponse, T anonymousObject, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(anonymousObject, CamelCase);
        var bytes = Encoding.UTF8.GetBytes(json);

        webResponse.Headers["Content-Type"] = "application/json";

        webResponse.HasBody = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    /// <summary>
    /// Clears the WebResponse.
    /// </summary>
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

    /// <summary>
    /// Applies a Vary header to the WebResponse. 
    /// If the Vary header already exists, it appends the new header name to the existing value.
    /// </summary>
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

    /// <summary>
    /// Starts streaming the response to the client by sending the HTTP status and headers.
    /// </summary>
    public static async Task StartStreaming(this WebResponse webResponse, CancellationToken cancellationToken = default)
    {
        if (webResponse.HasStarted)
        {
            return;
        }

        var outputStream = webResponse.OutputStream ?? throw new InvalidOperationException("Response output stream has not been set.");

        webResponse.IsStreaming = true;
        webResponse.HasBody = true;

        //streaming responses have no known content length
        webResponse.Headers.Remove("Content-Length");

        //enable HTTP chunked encoding
        webResponse.Headers["Transfer-Encoding"] = "chunked";

        var responseHeaders = $"HTTP/1.1 {webResponse.StatusCode} {webResponse.ResponsePhrase}\r\n" + string.Join("", webResponse.Headers.Select(h => $"{h.Key}: {h.Value}\r\n")) + "\r\n";

        var headerBytes = Encoding.ASCII.GetBytes(responseHeaders);

        //send the HTTP status and headers
        await outputStream.WriteAsync(headerBytes, cancellationToken);

        webResponse.HasStarted = true;
    }

    /// <summary>
    /// Writes a string to the response stream in chunked transfer encoding format.
    /// </summary>
    public static async Task WriteToStream(this WebResponse webResponse, string message, CancellationToken cancellationToken = default)
    {
        if (!webResponse.HasStarted)
        {
            throw new InvalidOperationException("Headers have not been sent yet.");
        }

        var outputStream = webResponse.OutputStream ?? throw new InvalidOperationException("Response output stream has not been set.");

        var messageBytes = Encoding.UTF8.GetBytes(message);

        //write the chunk length in hexadecimal
        var chunkLength = messageBytes.Length.ToString("X", CultureInfo.InvariantCulture);
        var chunkHeaderBytes = Encoding.ASCII.GetBytes($"{chunkLength}\r\n");

        //write the chunk header
        await outputStream.WriteAsync(chunkHeaderBytes, cancellationToken);
        //write the message
        await outputStream.WriteAsync(messageBytes, cancellationToken);
        //write the trailing CRLF
        await outputStream.WriteAsync("\r\n"u8.ToArray(), cancellationToken);
    }

    /// <summary>
    /// Flushes the response stream to ensure that all buffered data is sent to the client.
    /// </summary>
    public static async Task FlushStream(this WebResponse webResponse, CancellationToken cancellationToken = default)
    {
        var outputStream = webResponse.OutputStream ?? throw new InvalidOperationException("Response output stream has not been set.");

        //push bytes to the browser
        await outputStream.FlushAsync(cancellationToken);
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
        return EngineCookies.Contains(cookieName);
    }
}
