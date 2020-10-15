using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrereqClient
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            using var client = new TcpClient();
            //connect to local IP Address
            client.Connect(IPAddress.Loopback, port: 5000 );
         
            var stream = client.GetStream();

            var data = Encoding.UTF8.GetBytes("Hello I'm a new client. Are you the server?");
            // open the canal and send message
            stream.Write(data);
        }
    }
}
