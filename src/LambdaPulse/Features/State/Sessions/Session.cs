namespace LambdaPulse.Features.State.Sessions;

/// <summary>
/// User session storing key-value pairs for a user across multiple requests.
/// </summary>
public sealed class Session : IDisposable
{
    private readonly Dictionary<string, object> _data = new();

    //ensures only one request can access session at a time, avoiding race conditions
    private readonly SemaphoreSlim _gate = new(1);

    /// <summary>
    /// Session ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Indicates whether the session has not been accessed before.
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// Timestamp when the session was created (in UTC).
    /// </summary>
    public DateTimeOffset CreatedUtc { get; }

    /// <summary>
    /// Timestamp when the session was last accessed (in UTC).
    /// </summary>
    public DateTimeOffset LastAccessedUtc { get; set; }

    /// <summary>
    /// Initializes a new instance of the Session class with the specified session ID.
    /// </summary>
    public Session(string id)
    {
        Id = id;
        var now = DateTimeOffset.UtcNow;
        CreatedUtc = now;
        LastAccessedUtc = now;
    }

    /// <summary>
    /// Gets the value for the specified key.
    /// </summary>
    public async Task<T?> GetValue<T>(string key)
    {
        await _gate.WaitAsync();
        try
        {
            _data.TryGetValue(key, out var value);
            return (value is T typedValue) ? typedValue : default;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Sets the value for the specified key.
    /// </summary>
    public async Task SetValue<T>(string key, T value)
    {
        await _gate.WaitAsync();
        try
        {
            _data[key] = value!;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Removes the value for the specified key.
    /// </summary>
    public async Task RemoveValue(string key)
    {
        await _gate.WaitAsync();
        try
        {
            _data.Remove(key);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Disposes the session and releases any resources used by the session.
    /// </summary>
    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
