using Microsoft.VisualBasic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrereqServer
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            //create and connect to local IP Address
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine($"Server started!");
            var i = 1;
            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine($"Accepted client {i}!");
                var stream = client.GetStream();
                // need space to store what is reading
                // allocate space corresponding to the message sent by client
                byte[] data = new byte[client.ReceiveBufferSize];

                //stream returns a number of bytes read from the client
                var count = stream.Read(data);
                // convert bytes into string 
                var message = Encoding.UTF8.GetString(data, 0, count);

                Console.WriteLine($"New message from client {i}: {message}");
                
                i += 1;
            
            }
        }
    }
}
