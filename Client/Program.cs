using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;


namespace Client
{

    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("cid")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    class ClientProgram
    {
        private const int Port = 5000;

        public void Constraint_ConnectionWithoutRequest_ShouldConnect()
        {
            var client = Connect();
            Assert.True(client.Connected);
        }
        static void Main(string[] args)
        { 
        }
        public void Constraint_RequestWithoutMethod_MissingMethodError()
        {
            var client = Connect();

            client.SendRequest("{}");

            var response = client.ReadResponse();

            Assert.Contains("missing method", response.Status.ToLower());
        }

        private static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }

        private static TcpClient Connect()
        {
            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, Port);
            return client;
        }
    }
   

    public static class Util
    {
        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static T FromJson<T>(this string element)
        {
            return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static void SendRequest(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
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
    }

    }




