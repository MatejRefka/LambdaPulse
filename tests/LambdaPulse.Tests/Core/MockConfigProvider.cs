using LambdaPulse.Server.Configuration;
using LambdaPulse.Server.Configuration.Models;
using System.Text.Json;

namespace LambdaPulse.Tests.Core;

public class MockConfigProvider : IConfigProvider
{
    private readonly Config _config;
    public ServerConfig ServerConfig => _config.ServerConfig;

    public MockConfigProvider()
    {
        _config = LoadConfig();
    }

    private static Config LoadConfig()
    {
        var configText = File.ReadAllText("mockConfig.json");
        var configJson = JsonSerializer.Deserialize<Config>(configText) ?? throw new ApplicationException("Unable to parse json config");
        return configJson;
    }
}
