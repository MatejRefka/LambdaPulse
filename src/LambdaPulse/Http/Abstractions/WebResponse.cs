namespace LambdaPulse.Services.Http.Models;

public sealed class WebResponse
{
    public int? StatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public bool HasStarted { get; set; }
}
