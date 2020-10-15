using System;
using System.Net;
using System.Net.Sockets;

namespace PrereqClient
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            using var client = new TcpClient();
            //connect to local IP Address
            client.Connect(IPAddress.Loopback, port: 5000 );
        }
    }
}
