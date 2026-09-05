namespace LambdaPulse.Features.Authentication;

/// <summary>
/// GuestUser is a singleton, one shared instance reused across the application.
/// Saves memory allocation for each unauthenticated request.
/// </summary>
public sealed class GuestUser : IUser
{
    /// <summary>
    /// Stores the single instance of GuestUser. One instance per application.
    /// </summary>
    public static GuestUser Instance { get; } = new GuestUser();

    /// <summary>
    /// Unique id of the user. Always null for GuestUser.
    /// </summary>
    public string? Id => null;

    /// <summary>
    /// Indicates whether the user is authenticated. Always false for GuestUser.
    /// </summary>
    public bool IsAuthenticated => false;

    /// <summary>
    /// Roles granted to the user. Always empty for GuestUser.
    /// </summary>
    public HashSet<string> Roles => new();

    //private constructor prevents external instantiation
    private GuestUser()
    {
    }
}
