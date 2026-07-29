using LambdaPulse.Configuration;
using LambdaPulse.Features.Authentication;
using LambdaPulse.Features.Compression;
using LambdaPulse.Features.Logging;
using LambdaPulse.Features.Routing;
using LambdaPulse.Features.State.Cache;
using LambdaPulse.Features.State.Sessions;
using LambdaPulse.Hosting.Client;
using LambdaPulse.Hosting.Connection;
using LambdaPulse.Http.Parsing;
using LambdaPulse.Http.Reading;
using LambdaPulse.Http.Writing;
using LambdaPulse.Middleware;
using LambdaPulse.Middleware.Implementations;

using PulseInject;

namespace LambdaPulse.Hosting;

public static class ServerBuilder
{
    public static IWebServer Build(Action<IEndpointRegistry>? configureEndpoints = null, Action<DependencyContainer>? configureServices = null, Config? config = null)
    {
        config ??= new Config();
        ValidateConfig(config);

        //register services
        var container = new DependencyContainer();

        //register default implementation
        RegisterDefaultServices(container, config);

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

    private static void RegisterDefaultServices(DependencyContainer container, Config config)
    {
        container.AddSingleton(config);

        container.AddSingleton<IConnectionListener, ConnectionListener>();

        container.AddSingleton<IClientHandler, ClientHandler>();

        container.AddSingleton<IRequestReader, RequestReader>();
        container.AddSingleton<IRequestParser, RequestParser>();
        container.AddSingleton<IResponseWriter, ResponseWriter>();

        container.AddSingleton<IEngineLogger, ConsoleEngineLogger>();
        container.AddSingleton<ITraceRecorder, NullTraceRecorder>();
        container.AddSingleton<IPreSessionInitializer, PreSessionInitializer>();
        container.AddSingleton<ISessionStore, InMemorySessionStore>();
        container.AddSingleton<ICacheStore, InMemoryCacheStore>();
        container.AddSingleton<IAuthenticationScheme, SessionAuthenticationScheme>();

        container.AddSingleton<IEndpointRegistry, EndpointRegistry>();
        container.AddSingleton<IEndpointComparer, EndpointComparer>();

        container.AddSingleton<ICompressor, GZipCompressor>();

        container.AddSingleton<WebServer>();
    }

    private static void ValidateConfig(Config config)
    {
        var fileRootPath = config.ServerConfig.MiddlewareConfig.StaticFilesConfig.FileRootPath;
        var indexPageRelativePath = config.ServerConfig.MiddlewareConfig.SpaFallbackConfig.IndexPageRelativePath;
        var httpsRedirectionEnabled = config.ServerConfig.MiddlewareConfig.HttpsRedirectionConfig.IsEnabled;
        var cookieSecure = config.ServerConfig.MiddlewareConfig.SessionConfig.CookieSecure;

        if (indexPageRelativePath != null && fileRootPath == null)
        {
            throw new InvalidOperationException("A static file root path must be configured when SPA fallback is enabled.");
        }

        if (!httpsRedirectionEnabled && cookieSecure)
        {
            throw new InvalidOperationException("Secure session cookies require HTTPS redirection to be enabled.");
        }
    }
}
