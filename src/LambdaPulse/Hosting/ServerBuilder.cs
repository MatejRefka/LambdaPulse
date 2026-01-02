using LambdaPulse.Configuration;
using LambdaPulse.DI;
using LambdaPulse.Services;
using LambdaPulse.Services.Http;
using LambdaPulse.Services.Http.Routing;
using LambdaPulse.Services.Http.State;

namespace LambdaPulse;

public static class ServerBuilder
{
    public static WebServer Build(EndpointRegistry endpointRegistry)
    {
        //register services
        var container = new DependencyContainer();
        container.AddSingleton<IConfigProvider, ConfigProvider>();

        container.AddSingleton<IConnectionListener, ConnectionListener>();

        container.AddSingleton<IClientHandler, ClientHandler>();

        container.AddSingleton<IRequestReader, RequestReader>();
        container.AddSingleton<IRequestParser, RequestParser>();
        container.AddSingleton<IResponseWriter, ResponseWriter>();

        container.AddSingleton<ISessionStore, InMemorySessionStore>();

        container.AddSingleton(endpointRegistry);

        container.AddSingleton<WebServer>();

        //need the resolver itself to resolve MW dependencies
        var resolver = new DependencyResolver(container);
        container.AddSingleton(resolver);

        var webServer = resolver.GetService<WebServer>();

        return webServer ?? throw new InvalidOperationException("Cannot construct WebServer");
    }
}
