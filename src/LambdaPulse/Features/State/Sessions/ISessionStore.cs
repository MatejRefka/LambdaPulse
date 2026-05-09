namespace LambdaPulse.Engine.Features.State;

public interface ISessionStore
{
    Task<Session?> GetSession(string sessionId, CancellationToken cancellationToken);

    Task SaveSession(Session session, CancellationToken cancellationToken);
}
