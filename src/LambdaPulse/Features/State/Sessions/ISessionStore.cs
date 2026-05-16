namespace LambdaPulse.Engine.Features.State.Sessions;

public interface ISessionStore
{
    Task<Session?> GetSession(string sessionId, CancellationToken cancellationToken = default);

    Task SaveSession(Session session, CancellationToken cancellationToken = default);
}
