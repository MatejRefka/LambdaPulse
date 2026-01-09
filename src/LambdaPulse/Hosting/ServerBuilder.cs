using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.DI;
using LambdaPulse.Server.Features.Compression;
using LambdaPulse.Server.Features.Routing;
using LambdaPulse.Server.Features.State;
using LambdaPulse.Server.Hosting.Client;
using LambdaPulse.Server.Hosting.Connection;
using LambdaPulse.Server.Http.Parsing;
using LambdaPulse.Server.Http.Reading;
using LambdaPulse.Server.Http.Writing;

namespace LambdaPulse.Server.Hosting;

public static class ServerBuilder
{
    public static WebServer Build(Action<IEndpointRegistry>? configureEndpoints = null, Action<DependencyContainer>? configureServices = null)
    {
        //register services
        var container = new DependencyContainer();

        //register default implementation
        RegisterDefaultServices(container);

        //allow users to override default implementations
        configureServices?.Invoke(container);

        //need the resolver itself to resolve MW dependencies
        var resolver = new DependencyResolver(container);
        container.AddSingleton(resolver);

        //allow users to add endpoints to the resolved endpoint registry
        if (configureEndpoints != null)
        {
            var endpointRegistry = resolver.GetService<IEndpointRegistry>() ?? throw new InvalidOperationException("Cannot resolve IEndpointRegistry");

            configureEndpoints(endpointRegistry);
        }

        var webServer = resolver.GetService<WebServer>();

        return webServer ?? throw new InvalidOperationException("Cannot construct WebServer");
    }

    private static void RegisterDefaultServices(DependencyContainer container)
    {
        container.AddSingleton<IConfigProvider, ConfigProvider>();

        container.AddSingleton<IConnectionListener, ConnectionListener>();

        container.AddSingleton<IClientHandler, ClientHandler>();

        container.AddSingleton<IRequestReader, RequestReader>();
        container.AddSingleton<IRequestParser, RequestParser>();
        container.AddSingleton<IResponseWriter, ResponseWriter>();

        container.AddSingleton<ISessionStore, InMemorySessionStore>();

        container.AddSingleton<IEndpointRegistry, EndpointRegistry>();
        container.AddSingleton<IEndpointComparer, EndpointComparer>();

        container.AddSingleton<ICompressor, GZipCompressor>();

        container.AddSingleton<WebServer>();
    }
}
