namespace LambdaPulse.Server.Features.State;

public interface ISessionStore
{
    public Session? GetSession(string sessionId);

    public void SaveSession(Session session);
}
