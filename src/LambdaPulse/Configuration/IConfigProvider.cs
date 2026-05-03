using LambdaPulse.Engine.Configuration.Models;

namespace LambdaPulse.Engine.Configuration;

/// <summary>
/// Exposes config sections
/// </summary>
public interface IConfigProvider
{
    ServerConfig ServerConfig { get; }
}
