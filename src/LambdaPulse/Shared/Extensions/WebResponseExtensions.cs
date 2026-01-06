using LambdaPulse.Server.Services.Http.Models;
using System.Text;

namespace LambdaPulse.Server.Utility.Extensions;

public static class WebResponseExtensions
{
    public static async Task WriteBytesToBody(this WebResponse webResponse, byte[] bytes, CancellationToken cancellationToken)
    {
        webResponse.HasStarted = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }

    public static async Task WriteStringToBody(this WebResponse webResponse, string text, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(text);

        webResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        webResponse.HasStarted = true;
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }
}
