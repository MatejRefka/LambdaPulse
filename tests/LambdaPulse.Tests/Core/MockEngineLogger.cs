using LambdaPulse.Engine.Features.Logging;

namespace LambdaPulse.Tests.Core;

public class MockEngineLogger : IEngineLogger
{
    public void Log(LogLevel logLevel, string source, string message, Exception? exception = null) { }
}
