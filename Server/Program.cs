using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace AServer
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

    [Serializable]
    internal class JsonReaderException : Exception
    {
        public JsonReaderException()
        {
        }

        public JsonReaderException(string message) : base(message)
        {
        }

        public JsonReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected JsonReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    class AServerProgram
    {
        private const int Port = 5000;

        //public string Name { get; set; }

        static void Main(string[] args)
        {
            StartServerThread();
            Thread.Sleep(300);
        }
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
                        //each thread handles a client connection
                        var request = new Request();
                        var client = server.AcceptTcpClient();
                        var stream = client.GetStream();
                        // allocate space corresponding to the message sent by client
                        byte[] data = new byte[client.ReceiveBufferSize];

                        request = ChargeClientRequest(stream, data);
                        Console.WriteLine($"Thread {i} -- message from new client : {request.Method} , {request.Body}, {request.Path}, {request.Date}");
                        Console.WriteLine($"right or wrong method : {VerifyMethod(request)}");
                        Console.WriteLine($"right or wrong path : {VerifyPath(request)}");
                        Console.WriteLine($"right or wrong date : {VerifyDate(request)}");
                        Console.WriteLine($"right or wrong body : {VerifyBody(request)}");
                        i += 1;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }
            });
            thread.Start();

        }

        public static Request ChargeClientRequest(NetworkStream stream, byte[] data)
        {
            //stream returns a number of bytes read from the client
            var count = stream.Read(data);
            // convert bytes into string 
            var message = Encoding.UTF8.GetString(data, 0, count);
            //from JSON text to request object
            return JsonSerializer.Deserialize<Request>(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        //verify constraints
        public static bool VerifyMethod(Request req)
        {
            var method = req.Method.ToLower();
            bool test;
            switch (method)
            {
                case "create":
                    //Console.WriteLine("method create");
                    test = true;
                    break;
                case "read":
                    //Console.WriteLine("method read");
                    test = true;
                    break;
                case "update":
                    //Console.WriteLine("method update");
                    test = true;
                    break;
                case "delete":
                    //Console.WriteLine("method delete");
                    test = true;
                    break;
                case "echo":
                    //Console.WriteLine("method echo");
                    test = true;
                    break;
                default:
                    //Console.WriteLine("unknown method");
                    test = false;
                    break;
            }
            return test;
        }


        public static bool VerifyPath(Request req)
        {
            Regex pattern = new Regex(@"^/\w+/\w+(/\d+)?$");
            //MatchCollection match = pattern.Matches(req.Path);
            //for (int i = 0; i < match.Count; i++)
            //    Console.WriteLine(match[i].Value);
            return pattern.IsMatch(req.Path);
        }

        public static bool VerifyDate(Request req)
        {
            return TryToParse(req.Date);

        }

        public static bool VerifyBody(Request req)
        {
            var test = IsValidJson(req.Body);
            return test;

        }

        // Utils
        private static bool TryToParse(string value)
        {
            bool success = Int64.TryParse(value, out long number);
            if (success)
            {
                Console.WriteLine("Converted '{0}' to {1}.", value, number);
            }
            else
            {
                if (value == null) value = "";
                Console.WriteLine("Attempted conversion of '{0}' failed.", value);
            }
            return success;
        }

        public static string ToJson(object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        private static bool IsValidJson(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return false;
            }
            //Removes all leading and trailing white-space characters from the current string
            body = body.Trim();
            if ((body.StartsWith("{") && body.EndsWith("}")) || //For object
                (body.StartsWith("[") && body.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(body);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }



    }


}

   



}
