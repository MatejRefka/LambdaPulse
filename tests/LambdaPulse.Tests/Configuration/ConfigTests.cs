using LambdaPulse.Configuration;

namespace LambdaPulse.Tests.Configuration;

public class ConfigTests
{
    [Fact]
    public void CreateCompleteDefaultConfiguration()
    {
        //arrange

        //act
        var config = new Config();

        //assert
        Assert.Equal("127.0.0.1", config.ServerConfig.Address);
        Assert.Equal(8080, config.ServerConfig.Port);
        Assert.Equal(512, config.ServerConfig.BackLog);
        Assert.Equal(120_000, config.ServerConfig.RequestReadTimeoutMS);

        var middleware = config.ServerConfig.MiddlewareConfig;

        Assert.Equal(8_192, middleware.RequestLimitsConfig.MaxControlDataSizeBytes);
        Assert.Equal(32_768, middleware.RequestLimitsConfig.MaxHeaderSizeBytes);
        Assert.Equal(3_000_000, middleware.RequestLimitsConfig.MaxBodySizeBytes);
        Assert.Null(middleware.ConnectionConfig.RequestExecutionTimeoutMS);
        Assert.Empty(middleware.IpBlocklistConfig.BlockedIpAddresses);
        Assert.False(middleware.HttpsRedirectionConfig.IsEnabled);

        Assert.False(middleware.HstsConfig.IsEnabled);
        Assert.Equal(31_536_000, middleware.HstsConfig.MaxAge);
        Assert.False(middleware.HstsConfig.IncludeSubDomains);
        Assert.False(middleware.HstsConfig.Preload);

        Assert.True(middleware.SecurityConfig.XContentTypeOptions);
        Assert.Equal("strict-origin-when-cross-origin", middleware.SecurityConfig.ReferrerPolicy);
        Assert.Null(middleware.SecurityConfig.PermissionsPolicy);
        Assert.Null(middleware.SecurityConfig.CrossOriginOpenerPolicy);
        Assert.Null(middleware.SecurityConfig.CrossOriginResourcePolicy);
        Assert.Null(middleware.SecurityConfig.CrossOriginEmbedderPolicy);
        Assert.True(middleware.SecurityConfig.RemoveServerHeader);
        Assert.Equal(20, middleware.SessionConfig.IdleTimeoutMinutes);
        Assert.Equal(720, middleware.SessionConfig.AbsoluteTimeoutMinutes);
        Assert.False(middleware.SessionConfig.CookieSecure);

        Assert.Null(middleware.StaticFilesConfig.FileRootPath);
        Assert.Null(middleware.SpaFallbackConfig.IndexPageRelativePath);
        Assert.Empty(middleware.CorsConfig.AllowedOrigins);
        Assert.False(middleware.CorsConfig.AllowCredentials);
        Assert.Empty(middleware.CorsConfig.ExposedHeaders);
        Assert.Empty(middleware.CorsConfig.AllowedMethods);
        Assert.Empty(middleware.CorsConfig.AllowedHeaders);
        Assert.Equal(StringComparer.OrdinalIgnoreCase, middleware.CorsConfig.AllowedOrigins.Comparer);
        Assert.Equal(StringComparer.OrdinalIgnoreCase, middleware.CorsConfig.ExposedHeaders.Comparer);
        Assert.Equal(StringComparer.OrdinalIgnoreCase, middleware.CorsConfig.AllowedMethods.Comparer);
        Assert.Equal(StringComparer.OrdinalIgnoreCase, middleware.CorsConfig.AllowedHeaders.Comparer);
        Assert.Equal(600, middleware.CorsConfig.PreflightMaxAgeSeconds);
    }
}
