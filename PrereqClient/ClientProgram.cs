using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PrereqClient
{
    class PrereqClientProgram
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


            //response from the server
            data = new byte[client.ReceiveBufferSize];
            //stream returns a number of bytes read from the client
            var count = stream.Read(data);
            // convert bytes into string 
            var message = Encoding.UTF8.GetString(data, 0, count);
            Console.WriteLine($"New message from server: {message}");
        }
    }
}
