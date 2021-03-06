﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;


namespace ServerA
{
   
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
    enum StatusCode 
    {
        Ok = 1,
        Created = 2,
        Updated = 3,
        Bad_Request = 4,
        Not_Found = 5,
        Error = 6
    }

    enum Reason
    {
        Ok,
        Illegal,
        Missing,
        To_create,
        To_update,
    }

    internal static class AServerProgram
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

                        request = ReadRequest(stream, data);
                        Console.WriteLine($"Thread {i} -- message from new client : {request.Method} , {request.Body}, {request.Path}, {request.Date}");
                        var newEl = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        Console.WriteLine($"{ newEl.Id} , {newEl.Name}");
                        client.ReplyToRequest(request);
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

        public static Request ReadRequest(NetworkStream stream, byte[] data)
        {
            //stream returns a number of bytes read from the client
            var count = stream.Read(data);
            // convert bytes into string 
            var message = Encoding.UTF8.GetString(data, 0, count);
            //from JSON text to request object
            return JsonSerializer.Deserialize<Request>(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }


        public static void ReplyToRequest(this TcpClient client, Request req)
        {

            bool success;
            bool previous;
            List<string> res = new List<string>();
            var method = VerifyMethod(req);
            var path = VerifyPath(req);
            var date = VerifyDate(req);
            var body = VerifyBody(req);

            switch (path)
            {
                case 1:
                    res.Add($"{ (Reason)1 } path");
                    success = false;
                    break;
                case 2:
                    res.Add($"{ (Reason)2 } path");
                    success = false;
                    break;
                default:
                    success = true;
                    break;
            }
           
            previous = success;
            

            switch (date)
            {
                case 1:
                    res.Add($"{ (Reason)1 } date");
                    success = false;
                    break;
                case 2:
                    res.Add($"{ (Reason)2 } date");
                    success = false;
                    break;
                default:
                    success = true;
                    break;
            }
            
            previous = previous & success;
            

            switch (body)
            {
                case 1:
                    res.Add($"{ (Reason)1 } body");
                    success = false;
                    break;
                case 2:
                    res.Add($"{ (Reason)2 } body");
                    success = false;
                    break;
                default:
                    success = true;
                    break;
            }
            
            previous = previous & success;
            

            switch (method)
            {
                case 1:
                    res.Add($"{ (Reason)1 } method");
                    success = false;
                    break;
                case 2:
                    res.Add($"{ (Reason)2 } method");
                    success = false;
                    break;
                default:
                    success = true;
                    break;
            }
            previous = previous & success;

            success = previous;
            Console.WriteLine($"success? {success}");
            Console.WriteLine($"res? {res.Count}");
            if (success)
            {
                if (method == 3)
                {
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Created} Created",

                    };
                    Console.WriteLine($"response? {response}");
                    client.SendResponse(ToJson(response));
                }
                else if (method == 4)
                {
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Updated} Updated",

                    };
                    Console.WriteLine($"response? {response}");
                    client.SendResponse(ToJson(response));

                }
                else if (method == 0)
                {
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Ok} Ok",

                    };
                    Console.WriteLine($"response? {response}");
                    client.SendResponse(ToJson(response));
                }
            }
            else
            {
                var response = new
                {
                    Status = $"{ (int)StatusCode.Bad_Request} Bad Request",
                    Body = String.Join(", ", res.ToArray())
                };
                client.SendResponse(ToJson(response));
            }

        }


        public static void SendResponse(this TcpClient client, string response)
        {
            var msg = Encoding.UTF8.GetBytes(response);
            Console.WriteLine($"msg? {msg.Length}");
            client.GetStream().Write(msg, 0, msg.Length);
        }

        //verify constraints
        public static int VerifyMethod(Request req)
        {
            var method = req.Method.ToLower();
            int test;
            switch (method)
            {
                case "create":
                    //Console.WriteLine("method create");
                    test = (int)Reason.To_create;
                    break;
                case "read":
                    //Console.WriteLine("method read");
                    test = (int)Reason.Ok;
                    break;
                case "update":
                    //Console.WriteLine("method update");
                    test = (int)Reason.To_update;
                    break;
                case "delete":
                    //Console.WriteLine("method delete");
                    test = (int)Reason.Ok;
                    break;

                case "echo":
                    //Console.WriteLine("method echo");
                    test = (int)Reason.Ok;
                    break;
                case "":
                    //Console.WriteLine("method echo");
                    test = (int)Reason.Missing;
                    break;
                case null:
                    //Console.WriteLine("method echo");
                    test = (int)Reason.Missing;
                    break;
                default:
                    //Console.WriteLine("unknown method");
                    test = (int)Reason.Illegal;
                    break;
            }
            return test;
        }


        public static int VerifyPath(Request req)
        {
            int test;
            Regex pattern = new Regex(@"^/\w+/\w+(/\d+)?$");
            if (string.IsNullOrWhiteSpace(req.Path))
            {
                test = (int)Reason.Missing;
                return test;
            }
            else if(pattern.IsMatch(req.Path)){
                test = (int)Reason.Ok;
                return test;
                //MatchCollection match = pattern.Matches(req.Path);
                //for (int i = 0; i < match.Count; i++)
                //    Console.WriteLine(match[i].Value);
            }
            else
            {
                test = (int)Reason.Illegal;
                return test;
            }
            
            
        }

        public static int VerifyDate(Request req)
        {
            bool success = Int64.TryParse(req.Date, out long result);
            int test;
            if (success)
            {
                Console.WriteLine("Converted '{0}' to {1}.", req.Date, result);
                test = (int)Reason.Ok;
                return test;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(req.Date)) {
                    req.Date = ""; 
                    Console.WriteLine("Attempted conversion of '{0}' failed.", req.Date);
                    test = (int)Reason.Missing;
                    return test;
                }
                else
                {
                    Console.WriteLine("Attempted conversion of '{0}' failed.", req.Date);
                    test = (int)Reason.Illegal;
                    return test;
                }
               
            }
            
        }

        public static int VerifyBody(Request req)
        {
            int test;
            if(req.Method.ToLower() == "read")
            {
                switch(req.Body)
                {
                    case null:
                        test = (int)Reason.Ok;
                        break;
                    default:
                        test = (int)Reason.Illegal;
                        break;
                }
                return test;
            }
            else {
                test = IsValidJson(req.Body);
                return test;
            }
             
        }

        // Utils
       
        public static string ToJson(object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static int IsValidJson(string body)
        {
            int test;
            if (string.IsNullOrWhiteSpace(body))
            {
                test = (int)Reason.Missing;
                return test;
            }
            //Removes all leading and trailing white-space characters from the current string
            body = body.Trim();
            if ((body.StartsWith("{") && body.EndsWith("}")) || //For object
                (body.StartsWith("[") && body.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(body);
                    test = (int)Reason.Ok;
                    return test;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    Console.WriteLine("test1");
                    test = (int)Reason.Illegal;
                    return test;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("test2");
                    test = (int)Reason.Illegal;
                    return test;
                }
            }
            else
            {
                test = (int)Reason.Illegal;
                return test;
            }
        }



    }


}

   




