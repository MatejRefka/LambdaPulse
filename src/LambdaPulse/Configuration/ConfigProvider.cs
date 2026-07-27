using LambdaPulse.Configuration.Models;
using System.Text.Json;

namespace LambdaPulse.Configuration;

/// <summary>
/// Parses config.json into Config data model
/// </summary>
internal sealed class ConfigProvider : IConfigProvider
{
    private readonly Config _config;
    public ServerConfig ServerConfig => _config.ServerConfig;

    public ConfigProvider()
    {
        _config = LoadConfig();
    }

    private static Config LoadConfig()
    {
        var configText = File.Exists("config.dev.json") ? File.ReadAllText("config.dev.json") : File.ReadAllText("config.json");
        var configJson = JsonSerializer.Deserialize<Config>(configText) ?? throw new ApplicationException("Unable to parse json config.");
        return configJson;
    }
}
