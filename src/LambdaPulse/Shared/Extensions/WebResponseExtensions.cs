using LambdaPulse.Services.Http.Models;
using System.Text;

namespace LambdaPulse.Utility.Extensions;

public static class WebResponseExtensions
{
    public static async Task WriteToBody(this WebResponse webResponse, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);

        webResponse.Headers["Content-Type"] = "text/plain; charset=utf-8";
        await webResponse.Body.WriteAsync(bytes);
    }
}
