namespace LambdaPulse.Server.Features.Logging;

/// <summary>
/// Model representing a complete trace of an HTTP request and response. Used by the ITraceLogger.
/// Copy of request/response metadata is needed as the WebContext holds refereces to the TcpClient.
/// </summary>
public sealed class Trace
{
    public required DateTimeOffset TimestampStart { get; set; }
    public float DurationMs { get; set; }

    public required string RequestMethod { get; set; }
    public required string RequestPath { get; set; }
    public required string RequestProtocol { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; } = new();
    public Dictionary<string, string> RequestCookies { get; set; } = new();
    public string? RequestBody { get; set; }

    public int ResponseStatusCode { get; set; }
    public string? ResponsePhrase { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
    public Dictionary<string, string> ResponseCookies { get; set; } = new();
    public string? ResponseBody { get; set; }


    public List<MiddlewareStep> Steps { get; set; } = new();
}

public sealed class MiddlewareStep
{
    public required string Middleware { get; set; }
    public FlowDirection? Direction { get; set; }
    public required ExecutionEvent Event { get; set; }
    public required DateTimeOffset TimestampStart { get; set; }
    public int DurationMs { get; set; }
    public List<string> Logs { get; set; } = new();
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