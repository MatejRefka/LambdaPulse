namespace LambdaPulse.Server.Utility;

public interface ICompressor
{
    public string Encoding { get; }
    public byte[] Compress(byte[] data);
}
