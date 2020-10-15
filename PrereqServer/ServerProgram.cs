using System;
using System.Net;
using System.Net.Sockets;

namespace PrereqServer
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            //create and connect to local IP Address
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            var i = 1;
            while (true)
            {
               
                var client = server.AcceptTcpClient();
                Console.WriteLine($"Accepted client {i}");
                i += 1;
            }
        }
    }
}
