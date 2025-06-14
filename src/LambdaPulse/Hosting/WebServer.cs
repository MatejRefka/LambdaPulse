using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse
{
    public class WebServer
    {
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly int _backlog;


        public WebServer(IPAddress address, int port, int backlog)
        {
            _address = address;
            _port = port;
            _backlog = backlog;
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
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Byte[] buffer = new Byte[1024];
                        String requestString = string.Empty;

                        //abstraction for reading and sending bytes over the TCP connection
                        using NetworkStream stream = tcpClient.GetStream();

                        int bytesReadCount;
                        //read request stream bytes into buffer
                        while ((bytesReadCount = await stream.ReadAsync(buffer)) != 0)
                        {
                            //append buffer bytes into result string
                            requestString += Encoding.UTF8.GetString(buffer, 0, bytesReadCount);

                            //detect end of HTTP request
                            if (requestString.Contains("\r\n\r\n"))
                            {
                                break;
                            }
                        }

                        Console.WriteLine($"Request: {requestString}");


                        //---> PARSE THE REQUEST INTO WEBCONTEXT WRAPPER <---


                        //---> PIPE IT THROUGH MIDDLEWARE <---


                        //---> SEND BACK A RESPONSE <---
                        var responseOK = "HTTP/1.1 200 OK\r\n" +
                                       "Content-Type: text/plain\r\n" +
                                       "Content-Length: 2\r\n" +
                                       "\r\n" +
                                       "OK";
                        var responseBytes = Encoding.UTF8.GetBytes(responseOK);
                        await stream.WriteAsync(responseBytes);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                });
            }
        }

        private WebRequest ParseRequestString()
        {


            throw new NotImplementedException();
        }
    }
}
