namespace LambdaPulse.Engine.Features.Logging;

/// <summary>
/// Model representing a complete trace of a HTTP request and response, including failed request parse. 
/// Copy of request/response metadata is needed as the WebContext holds refereces to the TcpClient.
/// Used by the ITraceLogger. 
/// </summary>
public sealed class Trace
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public required DateTimeOffset TimestampStart { get; set; }
    public long DurationMs { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestProtocol { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public List<MiddlewareStep> Steps { get; set; } = new();
}

public sealed class MiddlewareStep
{
    public required string Middleware { get; set; }
    public FlowDirection? Direction { get; set; }
    public required ExecutionEvent Event { get; set; }
    public required DateTimeOffset TimestampStart { get; set; }
    public long DurationMs { get; set; }
    public List<string>? Logs { get; set; }
}

public enum FlowDirection
{
    Downstream,
    Upstream
}

public enum ExecutionEvent
{
    Success,
    ShortCircuit,
    Error
}