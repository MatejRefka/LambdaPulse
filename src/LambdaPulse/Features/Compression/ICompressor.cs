namespace LambdaPulse.Features.Compression;

public interface ICompressor
{
    string Encoding { get; }
    byte[] Compress(byte[] data);
}
