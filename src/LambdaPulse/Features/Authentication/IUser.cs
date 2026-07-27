namespace LambdaPulse.Features.Authentication;

public interface IUser
{
    string? Id { get; }
    bool IsAuthenticated { get; }
    HashSet<string> Roles { get; }
}
