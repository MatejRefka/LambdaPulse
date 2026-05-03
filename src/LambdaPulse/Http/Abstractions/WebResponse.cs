namespace LambdaPulse.Engine.Http.Abstractions;

public sealed class WebResponse
{
    public int? StatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();

    //Response cookie header contains key-value pair plus metadata. E.g. Set-Cookies: sessionId=123; Path=/; SameSite=Strict
    public List<string> Cookies { get; } = new();
    internal MemoryStream Body { get; } = new();
    public bool HasStarted { get; internal set; }
}