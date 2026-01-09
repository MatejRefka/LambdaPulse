namespace LambdaPulse.Server.Http.Abstractions;

public sealed class WebRequest
{
    public required string Method { get; set; }
    public required string Path { get; set; }
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public required string Protocol { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> Cookies { get; set; } = new();
    public string? Body { get; set; }
}
