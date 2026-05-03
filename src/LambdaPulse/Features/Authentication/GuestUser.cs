namespace LambdaPulse.Engine.Features.Authentication;

/// <summary>
/// GuestUser is a singleton, one shared instance reused across the application.
/// Saves memory allocation for each unauthenticated request.
/// </summary>
internal sealed class GuestUser : IUser
{
    public static GuestUser Instance { get; } = new GuestUser();
    public string? Id => null;
    public bool IsAuthenticated => false;
    public HashSet<string> Roles => new();
    private GuestUser()
    {
    }
}
