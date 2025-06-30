using LambdaPulse.Configuration.Models;

namespace LambdaPulse.Configuration
{
    /// <summary>
    /// Exposes config sections
    /// </summary>
    public interface IConfigProvider
    {
        public ServerConfig ServerConfig { get; }
    }
}
