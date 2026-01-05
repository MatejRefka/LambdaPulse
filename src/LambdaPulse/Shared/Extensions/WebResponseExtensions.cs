using LambdaPulse.Server.Services.Http.Models;
using System.Text;

namespace LambdaPulse.Server.Utility.Extensions;

public static class WebResponseExtensions
{
    public static async Task WriteToBody(this WebResponse webResponse, string text, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(text);

        webResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        await webResponse.Body.WriteAsync(bytes, cancellationToken);
    }
}
