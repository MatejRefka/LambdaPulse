using LambdaPulse.Engine.Configuration;
using System.Collections.Concurrent;

namespace LambdaPulse.Engine.Features.State;

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

    public Session? GetSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        if (now - session.LastAccessedUtc > _idleTimeout || now - session.CreatedUtc > _absoluteTimeout)
        {
            //session has expired
            RemoveSession(sessionId);
            return null;
        }

        //update last accessed time after session is read
        session.LastAccessedUtc = now;
        return session;
    }

    public void SaveSession(Session session)
    {
        _sessions[session.Id] = session;
        session.IsNew = false;
    }

    public void RemoveSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out var session);
        session?.Dispose();
    }
}
