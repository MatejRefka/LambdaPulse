namespace LambdaPulse.Http.Abstractions;

public sealed class WebResponse
{
    public int? StatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    //Response cookie header contains key-value pair plus metadata. E.g. Set-Cookies: sessionId=123; Path=/; SameSite=Strict
    public List<string> Cookies { get; } = new();
    internal MemoryStream Body { get; } = new();
    public bool HasBody { get; internal set; }
    //OutputStream used to write response in real-time to the client (SSE)
    internal Stream? OutputStream { get; set; }
    //response is being sent in real-time to the client (SSE)
    public bool IsStreaming { get; internal set; }
    //response has started being sent to the client (headers sent)
    public bool HasStarted { get; internal set; }

}