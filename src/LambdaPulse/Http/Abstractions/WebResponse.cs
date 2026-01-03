namespace LambdaPulse.Server.Services.Http.Models;

public sealed class WebResponse
{
    public int? StatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public List<string> Cookies { get; } = new();
    public MemoryStream Body { get; } = new MemoryStream();
    public bool HasStarted { get; set; }
}