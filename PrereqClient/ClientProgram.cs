using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace PrereqClient
{
    class PrereqClientProgram
    {
        public static void SendRequest(TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }

        public static string ToJson(object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        static void Main(string[] args)
        {
            for (int j = 0; j < 2; j++)
            {
                using var client = new TcpClient();
                //connect to local IP Address
                client.Connect(IPAddress.Loopback, port: 5000);

                //var stream = client.GetStream();

                //var data = Encoding.UTF8.GetBytes($"Hello I'm a new client {j}. Are you the server?");
                // open the canal and send message
                // stream.Write(data);
                var req = new
                {
                    method = "delete",
                    Path = "/api/categories/1234",
                    Date = $"{j}"
                };

                SendRequest(client,ToJson(req));

                /*
                //response from the server
                data = new byte[client.ReceiveBufferSize];
                //stream returns a number of bytes read from the client
                var count = stream.Read(data);
                // convert bytes into string 
                var message = Encoding.UTF8.GetString(data, 0, count);
                Console.WriteLine($"New message from server: {message}");*/
            }
        }

      
    }
}
