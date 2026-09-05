namespace LambdaPulse.Features.Compression;

/// <summary>
/// Compresses response data for use by the response compression middleware.
/// </summary>
public interface ICompressor
{
    /// <summary>
    /// Content encoding this compressor produces (e.g. "gzip", "br")
    /// </summary>
    string Encoding { get; }

    /// <summary>
    /// Compresses the given data.
    /// </summary>
    byte[] Compress(byte[] data);
}
