namespace LambdaPulse.Engine.Features.Compression;

public interface ICompressor
{
    string Encoding { get; }
    byte[] Compress(byte[] data);
}
