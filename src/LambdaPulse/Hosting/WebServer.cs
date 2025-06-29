using LambdaPulse.Configuration;
using LambdaPulse.Services;
using System.Net;
using System.Net.Sockets;

namespace LambdaPulse
{
    public class WebServer
    {
        private readonly IClientHandler _clientHandler;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly int _backlog;

        public WebServer(IClientHandler clientHandler, IConfigProvider configProvider)
        {
            _clientHandler = clientHandler;
            _address = IPAddress.Parse(configProvider.ServerConfig.Address);
            _port = configProvider.ServerConfig.Port;
            _backlog = configProvider.ServerConfig.BackLog;
        }

        public async Task StartServer()
        {
            //application-level setup
            var server = new TcpListener(_address, _port);

            //OS creates a socket in LISTEN state
            server.Start(_backlog);

            //listen forever, continuously accepting clients
            while (true)
            {
                //wait for OS to complete TCP handshake. TcpClient holds layer 4 connection (source IP+port, dest IP+port)
                using var tcpClient = await server.AcceptTcpClientAsync();

                //handle each client on a background thread
                try
                {
                    _ = _clientHandler.HandleClient(tcpClient);
                }
                catch (Exception ex)
                {
                    //single client exception should not take out the whole server
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
