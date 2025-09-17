using LambdaPulse.Services.Http.Models;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Services.Http
{
    public sealed class ResponseWriter : IResponseWriter
    {
        public async Task WriteHttpResponse(NetworkStream networkStream, WebResponse response)
        {
            var responseOK = "HTTP/1.1 200 OK\r\n" +
                           "Content-Type: text/plain\r\n" +
                           "Content-Length: 2\r\n" +
                           "\r\n" +
                           "OK";
            var responseBytes = Encoding.UTF8.GetBytes(responseOK);
            await networkStream.WriteAsync(responseBytes);
        }
    }
}
