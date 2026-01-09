using System.IO.Compression;

namespace LambdaPulse.Server.Features.Compression;

public sealed class GZipCompressor : ICompressor
{
    public string Encoding => "gzip";

    public byte[] Compress(byte[] data)
    {
        using var outputStream = new MemoryStream();

        //must dispose gzipStream before reading outputStream
        using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Fastest, leaveOpen: true))
        {
            gzipStream.Write(data, 0, data.Length);
        }

        return outputStream.ToArray();
    }
}
