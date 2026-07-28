using LambdaPulse.Configuration;
using LambdaPulse.Hosting;

namespace LambdaPulse.Tests.Core;

public class ServerBuilderTests
{
    [Fact]
    public void BuildWithDefaultConfiguration()
    {
        using var server = ServerBuilder.Build();

        Assert.NotNull(server);
    }

    [Fact]
    public void BuildWithConsumerConfiguration()
    {
        var config = new Config
        {
            ServerConfig = new ServerConfig
            {
                Port = 9000
            }
        };

        using var server = ServerBuilder.Build(config: config);

        Assert.NotNull(server);
    }
}
