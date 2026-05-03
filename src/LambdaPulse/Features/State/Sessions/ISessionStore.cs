namespace LambdaPulse.Engine.Features.State;

public interface ISessionStore
{
    Session? GetSession(string sessionId);

    void SaveSession(Session session);
}
