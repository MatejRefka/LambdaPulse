using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.DI;
using LambdaPulse.Server.Features.Compression;
using LambdaPulse.Server.Features.Routing;
using LambdaPulse.Server.Features.State;
using LambdaPulse.Server.Hosting.Clients;
using LambdaPulse.Server.Hosting.Connections;
using LambdaPulse.Server.Http.Parsing;
using LambdaPulse.Server.Http.Reading;
using LambdaPulse.Server.Http.Writing;

namespace LambdaPulse.Server.Hosting;

public static class ServerBuilder
{
    public static WebServer Build(EndpointRegistry? endpointRegistry = null)
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

        container.AddSingleton(endpointRegistry ?? new EndpointRegistry());

        container.AddSingleton<ICompressor, GZipCompressor>();

        container.AddSingleton<WebServer>();

        //need the resolver itself to resolve MW dependencies
        var resolver = new DependencyResolver(container);
        container.AddSingleton(resolver);

        var webServer = resolver.GetService<WebServer>();

        return webServer ?? throw new InvalidOperationException("Cannot construct WebServer");
    }
}
