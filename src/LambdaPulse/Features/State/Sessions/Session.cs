namespace LambdaPulse.Server.Services.Http.State;

public class Session : IDisposable
{
    private readonly Dictionary<string, object> _data = new();

    //ensures only one request can access session at a time, avoding race conditions
    private readonly SemaphoreSlim _gate = new(1);

    public string Id { get; }
    public bool IsNew { get; set; }
    public DateTimeOffset CreatedUtc { get; }
    public DateTimeOffset LastAccessedUtc { get; set; }

    public Session(string id)
    {
        Id = id;
        var now = DateTimeOffset.UtcNow;
        CreatedUtc = now;
    }

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

    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
