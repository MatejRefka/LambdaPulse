namespace LambdaPulse.Server.Services.Http.State;

public interface ISessionStore
{
    public Session? GetSession(string sessionId);

    public void SaveSession(Session session);
}
