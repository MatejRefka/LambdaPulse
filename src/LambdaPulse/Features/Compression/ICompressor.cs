namespace LambdaPulse.Server.Features.Compression;

public interface ICompressor
{
    public string Encoding { get; }
    public byte[] Compress(byte[] data);
}
