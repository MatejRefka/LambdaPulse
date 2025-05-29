using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LambdaPulse
{
    public class WebServer
    {
        private readonly int _port;

        public WebServer(int port)
        {
            _port = port;
        }

        public void StartServer()
        {
            var server = new TcpListener(IPAddress.Any, _port);

            //start listening on port
            server.Start();

            //buffer for reading data
            Byte[] buffer = new Byte[256];
            String data = string.Empty;

            //listening loop
            while (true)
            {
                //blocking call accepting the request
                using var client = server.AcceptTcpClient();

                //request stream
                using NetworkStream stream = client.GetStream();

                int bytesReadCount;

                while ((bytesReadCount = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    //append buffer bytes into result string
                    data += Encoding.UTF8.GetString(buffer, 0, bytesReadCount);

                    //detect end of HTTP request
                    if (data.Contains("\r\n\r\n"))
                    {
                        break;
                    }
                }

                Console.WriteLine($"Request: {data}");

                //parse the request into WebContext
                //pipe it through middleware
                //send back a response
            }
        }
    }
}
