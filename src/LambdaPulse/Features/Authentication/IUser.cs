namespace LambdaPulse.Engine.Features.Authentication.Abstractions;

public interface IUser
{
    string? Id { get; }
    bool IsAuthenticated { get; }
    HashSet<string> Roles { get; }
}
