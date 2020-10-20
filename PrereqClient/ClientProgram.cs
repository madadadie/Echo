using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrereqClient
{
    public class Response
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("cid")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    static class  PrereqClientProgram
    {
        public static void SendRequest(TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }

        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        private static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
        public static Response ReadResponse(this TcpClient client)
        {
            var strm = client.GetStream();
            //strm.ReadTimeout = 250;
            byte[] resp = new byte[2048];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);

                } while (bytesread == 2048);

                var responseData = Encoding.UTF8.GetString(memStream.ToArray());
                return JsonSerializer.Deserialize<Response>(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        }

        static void Main(string[] args)
        {
            for (int j = 0; j < 1; j++)
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
                    method = "echo",
                    Path = "/api/categories",
                    Date = "15078869",
                   Body = "Hello Word"
                    
                
                };

                SendRequest(client,ToJson(req));
                var response = client.ReadResponse();
                Console.WriteLine($"message from server -- {response.Status} {response.Body}");

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


