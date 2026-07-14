using LambdaPulse.Engine.Http.Abstractions;

namespace LambdaPulse.Engine.Features.State.Sessions;

public interface IPreSessionInitializer
{
    void Initialize(WebContext webContext);
}
