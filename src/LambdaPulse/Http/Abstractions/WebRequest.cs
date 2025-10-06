namespace LambdaPulse.Services.Http.Models;

public sealed class WebRequest
{
    public required string Method { get; set; }
    public required string Path { get; set; }
    public required string Protocol { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
}
