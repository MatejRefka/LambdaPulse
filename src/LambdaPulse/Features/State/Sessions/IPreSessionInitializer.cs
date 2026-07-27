using LambdaPulse.Http.Abstractions;

namespace LambdaPulse.Features.State.Sessions;

public interface IPreSessionInitializer
{
    void Initialize(WebContext webContext);
}
