namespace LambdaPulse.Features.State.Sessions;

/// <summary>
/// Defines operations for retrieving, saving and deleting sessions.
/// </summary>
public interface ISessionStore
{
    /// <summary>
    /// Retrieves the session for the given session ID.
    /// </summary>
    Task<Session?> GetSession(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the given session.
    /// </summary>
    Task SaveSession(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the session for the given session ID.
    /// </summary>
    Task DeleteSession(string sessionId, CancellationToken cancellationToken = default);
}
