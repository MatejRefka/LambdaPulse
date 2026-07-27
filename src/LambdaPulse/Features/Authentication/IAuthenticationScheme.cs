using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.Authentication;

public interface IAuthenticationScheme
{
    string SchemeName { get; }
    Task<IUser> Authenticate(WebContext webContext, CancellationToken cancellationToken = default);
}
