using LambdaPulse.Configuration.Models;

namespace LambdaPulse.Configuration
{
    public interface IConfigProvider
    {
        public ServerConfig ServerConfig { get; }
    }
}
