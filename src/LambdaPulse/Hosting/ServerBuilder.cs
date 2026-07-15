using LambdaPulse.Engine.Configuration;
using LambdaPulse.Engine.DI;
using LambdaPulse.Engine.Features.Authentication;
using LambdaPulse.Engine.Features.Compression;
using LambdaPulse.Engine.Features.Logging;
using LambdaPulse.Engine.Features.Routing;
using LambdaPulse.Engine.Features.State.Cache;
using LambdaPulse.Engine.Features.State.Sessions;
using LambdaPulse.Engine.Hosting.Client;
using LambdaPulse.Engine.Hosting.Connection;
using LambdaPulse.Engine.Http.Parsing;
using LambdaPulse.Engine.Http.Reading;
using LambdaPulse.Engine.Http.Writing;
using LambdaPulse.Engine.Middleware;
using LambdaPulse.Engine.Middleware.Implementations;

namespace LambdaPulse.Engine.Hosting;

public static class ServerBuilder
{
    public static IWebServer Build(Action<IEndpointRegistry>? configureEndpoints = null, Action<DependencyContainer>? configureServices = null)
    {
        //register services
        var container = new DependencyContainer();

        //register default implementation
        RegisterDefaultServices(container);

        //allow users to override default implementations
        configureServices?.Invoke(container);

        //need the resolver itself to resolve MW dependencies
        var resolver = new DependencyResolver(container);

        //allow users to add endpoints to the resolved endpoint registry
        if (configureEndpoints != null)
        {
            var endpointRegistry = resolver.GetService<IEndpointRegistry>() ?? throw new InvalidOperationException("Cannot resolve IEndpointRegistry.");

            configureEndpoints(endpointRegistry);
        }

        //construct the middleware pipeline, once per server instance
        var pipeline = new Pipeline(resolver)
                        .AddMiddleware<ExceptionMiddleware>()
                        .AddMiddleware<RequestLimitsMiddleware>()
                        .AddMiddleware<ConnectionMiddleware>()
                        .AddMiddleware<HttpsRedirectionMiddleware>()
                        .AddMiddleware<HstsMiddleware>()
                        .AddMiddleware<SecurityMiddleware>()
                        .AddMiddleware<CorsMiddleware>()
                        .AddMiddleware<SessionMiddleware>()
                        .AddMiddleware<RoutingMiddleware>()
                        .AddMiddleware<CsrfMiddleware>()
                        .AddMiddleware<ResponseCompressionMiddleware>()
                        .AddMiddleware<StaticFilesMiddleware>()
                        .AddMiddleware<SpaFallbackMiddleware>()
                        .AddMiddleware<AuthenticationMiddleware>()
                        .AddMiddleware<AuthorizationMiddleware>()
                        .AddMiddleware<ContentNegotiationMiddleware>()
                        .AddMiddleware<CacheMiddleware>()
                        .AddMiddleware<InvokeMiddleware>()
                        .AddMiddleware<TerminationMiddleware>()
                        .Build();
        container.AddSingleton(pipeline);

        var webServer = resolver.GetService<WebServer>();

        return webServer ?? throw new InvalidOperationException("Cannot construct WebServer.");
    }

    private static void RegisterDefaultServices(DependencyContainer container)
    {
        container.AddSingleton<IConfigProvider, ConfigProvider>();

        container.AddSingleton<IConnectionListener, ConnectionListener>();

        container.AddSingleton<IClientHandler, ClientHandler>();

        container.AddSingleton<IRequestReader, RequestReader>();
        container.AddSingleton<IRequestParser, RequestParser>();
        container.AddSingleton<IResponseWriter, ResponseWriter>();

        container.AddSingleton<IEngineLogger, ConsoleEngineLogger>();
        container.AddSingleton<ITraceLogger, NullTraceLogger>();
        container.AddSingleton<IPreSessionInitializer, PreSessionInitializer>();
        container.AddSingleton<ISessionStore, InMemorySessionStore>();
        container.AddSingleton<ICacheStore, InMemoryCacheStore>();
        container.AddSingleton<IAuthenticationScheme, SessionAuthenticationScheme>();

        container.AddSingleton<IEndpointRegistry, EndpointRegistry>();
        container.AddSingleton<IEndpointComparer, EndpointComparer>();

        container.AddSingleton<ICompressor, GZipCompressor>();

        container.AddSingleton<WebServer>();
    }
}
