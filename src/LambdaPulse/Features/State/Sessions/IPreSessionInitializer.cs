using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.State.Sessions;

/// <summary>
/// Ensures a pre-session token is created for the request before the session pipeline runs.
/// </summary>
public interface IPreSessionInitializer
{
    /// <summary>
    /// Initializes the pre-session token for the request.
    /// </summary>
    void Initialize(WebContext webContext);
}
