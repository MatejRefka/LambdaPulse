using LambdaPulse.Server.Configuration.Models;

namespace LambdaPulse.Server.Configuration;

/// <summary>
/// Exposes config sections
/// </summary>
public interface IConfigProvider
{
    public ServerConfig ServerConfig { get; }
}
