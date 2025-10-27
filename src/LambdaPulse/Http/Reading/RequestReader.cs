using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Services.Http;

public sealed class RequestReader : IRequestReader
{
    public async Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        string requestString = string.Empty;

        int bytesReadCount;

        //read request stream bytes into buffer
        while ((bytesReadCount = await networkStream.ReadAsync(buffer, cancellationToken)) != 0)
        {
            //append buffer bytes into result string
            requestString += Encoding.UTF8.GetString(buffer, 0, bytesReadCount);

            //detect end of HTTP request
            if (requestString.Contains("\r\n\r\n"))
            {
                break;
            }
        }

        return requestString;
    }
}
