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
        Assert.Equal(100, config.ServerConfig.BackLog);
        Assert.Equal(120_000, config.ServerConfig.ReadTimeoutMS);

        var middleware = config.ServerConfig.MiddlewareConfig;

        Assert.Equal(3_000_000, middleware.RequestLimitsConfig.MaxControlDataSizeBytes);
        Assert.Equal(15_000, middleware.RequestLimitsConfig.MaxHeaderSizeBytes);
        Assert.Equal(3_000_000, middleware.RequestLimitsConfig.MaxBodySizeBytes);
        Assert.Null(middleware.ConnectionConfig.RequestExecutionTimeoutMS);
        Assert.Empty(middleware.IpBlocklistConfig.BlockedIpAddresses);
        Assert.False(middleware.HttpsRedirectionConfig.IsEnabled);

        Assert.False(middleware.HstsConfig.IsEnabled);
        Assert.Equal(63_072_000, middleware.HstsConfig.MaxAge);
        Assert.True(middleware.HstsConfig.IncludeSubDomains);
        Assert.False(middleware.HstsConfig.Preload);

        Assert.True(middleware.SecurityConfig.XContentTypeOptions);
        Assert.Equal("strict-origin-when-cross-origin", middleware.SecurityConfig.ReferrerPolicy);
        Assert.Equal("camera=(), microphone=(), geolocation=()", middleware.SecurityConfig.PermissionsPolicy);
        Assert.Equal("same-origin", middleware.SecurityConfig.CrossOriginOpenerPolicy);
        Assert.Equal("same-origin", middleware.SecurityConfig.CrossOriginResourcePolicy);
        Assert.Equal("require-corp", middleware.SecurityConfig.CrossOriginEmbedderPolicy);
        Assert.True(middleware.SecurityConfig.RemoveServerHeader);
        Assert.Equal(20, middleware.SessionConfig.IdleTimeoutMinutes);
        Assert.Equal(720, middleware.SessionConfig.AbsoluteTimeoutMinutes);
        Assert.True(middleware.SessionConfig.CookieSecure);

        Assert.Null(middleware.StaticFilesConfig.FileRootPath);
        Assert.Null(middleware.SpaFallbackConfig.IndexPageRelativePath);
        Assert.Empty(middleware.CorsConfig.AllowedOrigins);
        Assert.False(middleware.CorsConfig.AllowCredentials);
        Assert.Empty(middleware.CorsConfig.ExposedHeaders);
        Assert.True(middleware.CorsConfig.AllowedMethods.SetEquals(["GET", "POST", "PUT", "DELETE", "OPTIONS"]));
        Assert.True(middleware.CorsConfig.AllowedHeaders.SetEquals(["Authorization", "Content-Type", "X-CSRF-Token"]));
        Assert.Contains("get", middleware.CorsConfig.AllowedMethods);
        Assert.Contains("content-type", middleware.CorsConfig.AllowedHeaders);
        Assert.Equal(60, middleware.CorsConfig.PreflightMaxAgeSeconds);
    }
}
