## LambdaPulse

LambdaPulse is a lightweight HTTP/1.1 web server engine with a request/response pipeline supporting 20+ pluggable middleware components.

## Features

- Concurrent connection handling
- Persistent keep-alive connections supporting multiple sequential requests per connection
- Configurable request limits, IP-based blocking, and execution timeouts
- HTTPS redirection and HSTS
- CORS and configurable security headers
- Cookie handling and in-memory session state
- CSRF protection for state-changing requests
- Static file serving with SPA fallback
- Session-based authentication and authorization
- Content negotiation and automatic Gzip response compression
- In-memory response caching
- Chunked response streaming
- Engine logging and per-request tracing
- Dependency injection powered by [PulseInject](https://github.com/MatejRefka/PulseInject)

## Installation
LambdaPulse is available as a pre-release package from [NuGet.org](https://www.nuget.org/packages/LambdaPulse)

```bash
dotnet add package LambdaPulse --version 0.1.0-beta.3
```

> **Beta:** LambdaPulse is under active development and its public API may change before version 1.0.
