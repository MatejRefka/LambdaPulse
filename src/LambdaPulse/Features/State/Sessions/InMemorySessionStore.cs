using LambdaPulse.Configuration;
using System.Collections.Concurrent;

namespace LambdaPulse.Features.State.Sessions;

internal sealed class InMemorySessionStore : ISessionStore
{
    //thread-safe against multiple concurrent requests
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private readonly TimeSpan _absoluteTimeout;
    private readonly TimeSpan _idleTimeout;

    public InMemorySessionStore(IConfigProvider configProvider)
    {
        _absoluteTimeout = TimeSpan.FromMinutes(configProvider.ServerConfig.MiddlewareConfig.SessionMiddleware.AbsoluteTimeoutMinutes);
        _idleTimeout = TimeSpan.FromMinutes(configProvider.ServerConfig.MiddlewareConfig.SessionMiddleware.IdleTimeoutMinutes);
    }

    public Task<Session?> GetSession(string sessionId, CancellationToken cancellation = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<Session?>(null);
        }

        var now = DateTimeOffset.UtcNow;

        if (now - session.LastAccessedUtc > _idleTimeout || now - session.CreatedUtc > _absoluteTimeout)
        {
            //session has expired
            _sessions.TryRemove(sessionId, out _);
            session?.Dispose();
            return Task.FromResult<Session?>(null);
        }

        //update last accessed time after session is read
        session.LastAccessedUtc = now;
        return Task.FromResult<Session?>(session);
    }

    public Task SaveSession(Session session, CancellationToken cancellationToken = default)
    {
        _sessions[session.Id] = session;
        session.IsNew = false;
        return Task.CompletedTask;
    }

    public Task DeleteSession(string sessionId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            session.Dispose();
        }

        return Task.CompletedTask;
    }
}
