namespace LambdaPulse.Server.Services.Http.Models;

public sealed class WebResponse
{
    public int? StatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public List<string> Cookies { get; } = new();
    internal MemoryStream Body { get; } = new();
    public bool HasStarted { get; internal set; }
}