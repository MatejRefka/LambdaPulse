using LambdaPulse.Middleware;
using LambdaPulse.Middleware.Implementations;
using LambdaPulse.Services.Http;
using System.Net.Sockets;

namespace LambdaPulse.Services
{
    public sealed class ClientHandler : IClientHandler
    {
        private readonly IRequestReader _requestReader;
        private readonly IRequestParser _requestParser;
        private readonly IResponseWriter _responseWriter;

        public ClientHandler(IRequestReader requestReader, IRequestParser requestParser, IResponseWriter responseWriter)
        {
            _requestReader = requestReader;
            _requestParser = requestParser;
            _responseWriter = responseWriter;
        }

        public async Task HandleClient(TcpClient tcpClient)
        {
            try
            {
                //abstraction for reading and sending bytes over the TCP connection
                await using var networkStream = tcpClient.GetStream();

                var requestString = await _requestReader.ReadHttpRequest(networkStream);

                var webContext = _requestParser.ParseHttpRequest(requestString);

                //construct the middleware pipeline
                var pipeline = new Pipeline()
                                .AddMiddleware<ExceptionHandler>()
                                .AddMiddleware<Logging>()
                                .AddMiddleware<RequestLimits>()
                                .AddMiddleware<Connection>()
                                .AddMiddleware<HttpsRedirection>()
                                .AddMiddleware<HSTS>()
                                .AddMiddleware<Security>()
                                .AddMiddleware<State>()
                                .AddMiddleware<CSRF>()
                                .AddMiddleware<StaticFiles>()
                                .AddMiddleware<ResponseCompression>()
                                .AddMiddleware<Routing>()
                                .AddMiddleware<CORS>()
                                .AddMiddleware<Authentication>()
                                .AddMiddleware<Authorization>()
                                .AddMiddleware<ContentNegotiation>()
                                .AddMiddleware<Endpoint>()
                                .Build();

                //Invoke the delegate
                await pipeline(webContext);

                await _responseWriter.WriteHttpResponse(networkStream, webContext.WebResponse);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}