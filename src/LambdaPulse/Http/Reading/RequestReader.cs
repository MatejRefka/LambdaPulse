using System.Net.Sockets;
using System.Text;

namespace LambdaPulse.Engine.Http.Reading;

internal sealed class RequestReader : IRequestReader
{
    public async Task<string> ReadHttpRequest(NetworkStream networkStream, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[1024];
        var requestBuilder = new StringBuilder();

        int bytesReadCount;

        //read request stream bytes into buffer
        while ((bytesReadCount = await networkStream.ReadAsync(buffer, cancellationToken)) != 0)
        {
            //append buffer bytes into result string
            requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesReadCount));
            var requestString = requestBuilder.ToString();

            //detect end of HTTP request headers, e.g. "GET / HTTP/1.1\r\nContent-Length: 5\r\n\r\nhello"
            if (requestString.Contains("\r\n\r\n"))
            {
                //extract header section from request string
                var headerEndIndex = requestString.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase);
                var headerSection = requestString[..headerEndIndex];

                //content length to find how many body bytes to read after headers
                var contentLength = 0;

                foreach (var line in headerSection.Split("\r\n"))
                {
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(line["Content-Length:".Length..].Trim(), out contentLength);
                        break;
                    }
                }

                //calculate how many body bytes are remaining to read after headers
                var bodyBytesCount = contentLength - (Encoding.UTF8.GetByteCount(requestString) - Encoding.UTF8.GetByteCount(headerSection) - 4);

                while (bodyBytesCount > 0)
                {
                    //read body bytes into buffer
                    bytesReadCount = await networkStream.ReadAsync(buffer, cancellationToken);
                    if (bytesReadCount == 0)
                    {
                        break;
                    }

                    //append body bytes into result string
                    requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesReadCount));
                    bodyBytesCount -= bytesReadCount;
                }

                break;
            }
        }

        return requestBuilder.ToString();
    }
}