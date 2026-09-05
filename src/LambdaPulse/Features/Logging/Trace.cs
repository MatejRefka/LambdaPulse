namespace LambdaPulse.Features.Logging;

/// <summary>
/// Model representing a complete trace of a HTTP request and response, including failed request parse. 
/// Copy of request/response metadata is needed as the WebContext holds refereces to the TcpClient.
/// Used by the ITraceLogger. 
/// </summary>
public sealed class Trace
{
    /// <summary>
    /// Unique identifier for the trace.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Indicates whether the trace is enabled for recording.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Pre-session token associated with the trace.
    /// </summary>
    public string? PreSessionToken { get; set; }

    /// <summary>
    /// Anonymous session token associated with the trace.
    /// </summary>
    public string? AnonymousSessionToken { get; set; }

    /// <summary>
    /// Associated user ID for the trace.
    /// </summary>
    public string? AssociatedUserId { get; set; }

    /// <summary>
    /// User ID associated with the trace.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Timestamp when the trace started.
    /// </summary>
    public required DateTimeOffset TimestampStart { get; set; }

    /// <summary>
    /// Duration of the trace in milliseconds.
    /// </summary>
    public float DurationMs { get; set; }

    /// <summary>
    /// HTTP request method the trace is for.
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// HTTP request path the trace is for.
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP request protocol the trace is for.
    /// </summary>
    public string? RequestProtocol { get; set; }

    /// <summary>
    /// HTTP response status code the trace is for.
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// HTTP response phrase the trace is for.
    /// </summary>
    public string? ResponsePhrase { get; set; }

    /// <summary>
    /// List of middleware steps that occurred during the trace.
    /// </summary>
    public List<MiddlewareStep> Steps { get; set; } = new();
}

/// <summary>
/// Records what happened at a single middleware, for a single direction.
/// </summary>
public sealed class MiddlewareStep
{
    /// <summary>
    /// Name of the middleware where the step occurred.
    /// </summary>
    public required string Middleware { get; set; }

    /// <summary>
    /// Direction of the flow for this middleware step (Downstream or Upstream).
    /// </summary>
    public FlowDirection? Direction { get; set; }

    /// <summary>
    /// Event that occurred during this middleware step (Success, ShortCircuit, or Error).
    /// </summary>
    public required ExecutionEvent Event { get; set; }

    /// <summary>
    /// Timestamp when the middleware step started.
    /// </summary>
    public required DateTimeOffset TimestampStart { get; set; }

    /// <summary>
    /// Duration of the middleware step in milliseconds.
    /// </summary>
    public float DurationMs { get; set; }

    /// <summary>
    /// Individual actions that occurred within this middleware, in order.
    /// </summary>
    public List<string>? Logs { get; set; }
}

/// <summary>
/// Direction of the flow for this middleware step. Either Downstream (request) or Upstream (response).
/// </summary>
public enum FlowDirection
{
    /// <summary>
    /// Downstream flow - request.
    /// </summary>
    Downstream,

    /// <summary>
    /// Upstream flow - response.
    /// </summary>
    Upstream
}

/// <summary>
/// Event that occurred during this middleware step. Either Success, ShortCircuit, or Error.
/// </summary>
public enum ExecutionEvent
{
    /// <summary>
    /// Middleware executed successfully without any issues.
    /// </summary>
    Success,

    /// <summary>
    /// The request flow stops here. The response flow begins back upstream.
    /// </summary>
    ShortCircuit,

    /// <summary>
    /// The request flow stops here due to an error. The response flow begins back upstream, picked up by error handling middleware.
    /// </summary>
    Error
}