using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Features.Authentication;

public interface IAuthenticationScheme
{
    string SchemeName { get; }
    Task<IUser> Authenticate(WebContext webContext, CancellationToken cancellationToken = default);
}
