namespace LambdaPulse.Engine.Features.Authentication.Abstractions;

internal sealed class AuthenticatedUser : IUser
{
    public string? Id { get; }
    public bool IsAuthenticated => true;
    public HashSet<string> Roles { get; }
    public AuthenticatedUser(string id, HashSet<string>? roles = null)
    {
        Id = id;
        Roles = (roles != null) ? new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
