using LambdaPulse.Configuration;
using LambdaPulse.Hosting;

namespace LambdaPulse.Tests.Core;

public class ServerBuilderTests
{
    [Fact]
    public void BuildWithDefaultConfiguration()
    {
        //arrange & act
        using var server = ServerBuilder.Build();

        //assert
        Assert.NotNull(server);
    }

    [Fact]
    public void BuildWithConsumerConfiguration()
    {
        //arrange
        var config = new Config
        {
            ServerConfig = new ServerConfig
            {
                Port = 9000
            }
        };

        //act
        using var server = ServerBuilder.Build(config: config);

        //assert
        Assert.NotNull(server);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectBlankStaticFileRoot(string fileRootPath)
    {
        //arrange
        var config = CreateConfig(fileRootPath, null);

        //act
        var exception = Assert.Throws<ArgumentException>(() => ServerBuilder.Build(config: config));

        //assert
        Assert.Contains("static file root", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectBlankSpaIndexPath(string indexPageRelativePath)
    {
        //arrange
        var config = CreateConfig(".", indexPageRelativePath);

        //act
        var exception = Assert.Throws<ArgumentException>(() => ServerBuilder.Build(config: config));

        //assert
        Assert.Contains("SPA index page", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RejectSpaFallbackWithoutStaticFileRoot()
    {
        //arrange
        var config = CreateConfig(null, "/index.html");

        //act
        var exception = Assert.Throws<InvalidOperationException>(() => ServerBuilder.Build(config: config));

        //assert
        Assert.Contains("static file root", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildWithStaticFilesAndNoSpaFallback()
    {
        //arrange
        var config = CreateConfig(".", null);

        //act
        using var server = ServerBuilder.Build(config: config);

        //assert
        Assert.NotNull(server);
    }

    [Fact]
    public void RejectSecureCookieWithoutHttpsRedirection()
    {
        //arrange
        var config = new Config
        {
            ServerConfig = new ServerConfig
            {
                MiddlewareConfig = new MiddlewareConfig
                {
                    SessionConfig = new SessionConfig
                    {
                        CookieSecure = true
                    }
                }
            }
        };

        //act
        var exception = Assert.Throws<InvalidOperationException>(() => ServerBuilder.Build(config: config));

        //assert
        Assert.Contains("secure session cookies", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Config CreateConfig(string? fileRootPath, string? indexPageRelativePath)
    {
        return new Config
        {
            ServerConfig = new ServerConfig
            {
                MiddlewareConfig = new MiddlewareConfig
                {
                    StaticFilesConfig = new StaticFilesConfig
                    {
                        FileRootPath = fileRootPath
                    },
                    SpaFallbackConfig = new SpaConfig
                    {
                        IndexPageRelativePath = indexPageRelativePath
                    }
                }
            }
        };
    }
}
