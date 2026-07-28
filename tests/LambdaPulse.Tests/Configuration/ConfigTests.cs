using LambdaPulse.Configuration;

namespace LambdaPulse.Tests.Configuration;

public class ConfigTests
{
    [Fact]
    public void CreateCompleteDefaultConfiguration()
    {
        var config = new Config();

        Assert.Equal("127.0.0.1", config.ServerConfig.Address);
        Assert.Equal(8080, config.ServerConfig.Port);
        Assert.Equal(100, config.ServerConfig.BackLog);
        Assert.Equal(120_000, config.ServerConfig.ReadTimeoutMS);

        var middleware = config.ServerConfig.MiddlewareConfig;

        Assert.Equal(3_000_000, middleware.RequestLimitsConfig.MaxControlDataSizeBytes);
        Assert.Equal(15_000, middleware.RequestLimitsConfig.MaxHeaderSizeBytes);
        Assert.Equal(3_000_000, middleware.RequestLimitsConfig.MaxBodySizeBytes);
        Assert.Equal(10_000, middleware.ConnectionConfig.RequestExecutionTimeoutMS);
        Assert.Empty(middleware.IpBlocklistConfig.BlockedIpAddresses);
        Assert.False(middleware.HttpsRedirectionConfig.IsEnabled);

        Assert.Equal(63_072_000, middleware.HstsConfig.MaxAge);
        Assert.True(middleware.HstsConfig.IncludeSubDomains);
        Assert.False(middleware.HstsConfig.Preload);

        Assert.True(middleware.SecurityConfig.XContentTypeOptions);
        Assert.True(middleware.SecurityConfig.RemoveServerHeader);
        Assert.Equal(20, middleware.SessionConfig.IdleTimeoutMinutes);
        Assert.Equal(720, middleware.SessionConfig.AbsoluteTimeoutMinutes);
        Assert.True(middleware.SessionConfig.CookieSecure);

        Assert.Equal("", middleware.StaticFilesConfig.FileRootPath);
        Assert.Equal("", middleware.SpaFallbackConfig.IndexPageRelativePath);
        Assert.Contains("http://localhost:8081", middleware.CorsConfig.AllowedOrigins);
        Assert.True(middleware.CorsConfig.AllowCredentials);
        Assert.Contains("X-Request-Id", middleware.CorsConfig.ExposedHeaders);
        Assert.Equal(60, middleware.CorsConfig.PreflightMaxAgeSeconds);
    }
}
