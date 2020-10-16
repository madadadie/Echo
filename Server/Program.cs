using Microsoft.VisualBasic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Server
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

    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }
    }



    class ServerProgram
    {
        private const int Port = 5000;
        private static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
        //public string Name { get; set; }


        //method to create listener threads
        public static void StartServerThread()
        {
            var server = new TcpListener(IPAddress.Loopback, Port);
            server.Start();
            Console.WriteLine($"Started server on port {Port}!");
            var i = 1;
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var client = server.AcceptTcpClient();
                        var stream = client.GetStream();
                        // need space to store what is reading
                        // allocate space corresponding to the message sent by client
                        byte[] data = new byte[client.ReceiveBufferSize];
                        //stream returns a number of bytes read from the client
                        var count = stream.Read(data);
                        // convert bytes into string 
                        var message = Encoding.UTF8.GetString(data, 0, count);
                        Console.WriteLine($"Thread {i} -- message from new client : {message}");
                        i += 1;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }
            });
            thread.Start();
            //thread.Name = $" child{i}";
        }
        static void Main(string[] args)
        {
            StartServerThread();
            Thread.Sleep(300);
        }


    }

   



}
