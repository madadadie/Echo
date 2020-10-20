using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;


namespace ServerB
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
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("body")]
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

    static class ServerBProgram
    {
        private const int Port = 5000;

        //public string Name { get; set; }

        static void Main(string[] args)
        {
            // Create a new dictionary of strings, with int keys.
            Dictionary<int, string> category = new Dictionary<int, string>();
            init(category);
            StartServerThread(category);
            Thread.Sleep(300);

         
        }

       
        static void init(Dictionary<int, string> category)
        {
           
            category.Add(1, "Beverages");
            category.Add(2, "Condiments");
            category.Add(3, "Confections");
        }
        //method to create listener threads
        public static void StartServerThread(Dictionary<int, string> category)
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
                       
                        client.ReplyToRequest(request, category);
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


        public static void ReplyToRequest(this TcpClient client, Request req, Dictionary<int, string> category)
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
            if (success)
            {
                if (method == 3)
                {
                    var newEl = JsonSerializer.Deserialize<Category>(req.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    
                    category.Insert(newEl.Name);
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Created} Created",
                        Body = new {
                            cid =category.Keys.Last(),
                            name= newEl.Name
                        }.ToJson()
                        
                    };
                    
                    client.SendResponse(response.ToJson());
                }
                else if (method == 4)
                {
                    string[] str = req.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    var id = int.Parse(str[str.Length - 1]);
                    var newEl = JsonSerializer.Deserialize<Category>(req.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    if (id != newEl.Id)
                    {
                        var response = new
                        {
                            Status = $"{ (int)StatusCode.Not_Found} Not Found"

                        };
                        client.SendResponse(response.ToJson());
                    }
                    else 
                    {
                        if (category.Update(newEl.Id, newEl.Name))
                        {
                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Updated} Updated"

                            };
                            client.SendResponse(response.ToJson());
                        }
                        else
                        {
                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Not_Found} Not Found"

                            };
                            client.SendResponse(response.ToJson());
                        }
                    }
                   
                    

                }
                else if (method == 0)
                {
                   
                    if (req.Method.ToLower() == "read")
                    {
                        string [] str = req.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                        bool parsenum = int.TryParse(str[str.Length - 1], out int id);
                        if (!parsenum)
                            
                        {

                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Ok} Ok",
                                Body = GetAll(category).ToJson()

                            };

                            client.SendResponse(response.ToJson());
                        }
                        else
                        {
                            if (category.FindById(id)=="")
                            {
                                var response = new
                                {
                                    Status = $"{ (int)StatusCode.Not_Found} Not Found"

                                };

                                client.SendResponse(response.ToJson());
                            }
                            else {
                                var response = new
                                {
                                    Status = $"{ (int)StatusCode.Ok} Ok",
                                    Body = new 
                                    {  
                                        cid = id,
                                        name = category.FindById(id) 
                                    }.ToJson()

                                };

                                client.SendResponse(response.ToJson());
                            }
                            
                        }

                    }
                    else if (req.Method.ToLower() == "delete")
                    {
                        string[] str = req.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                        var id  = int.Parse(str[str.Length - 1]);
                        if (category.Delete(id))
                        {
                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Ok} Ok"

                            };
                            client.SendResponse(response.ToJson());
                        }
                        else
                        {
                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Not_Found} Not Found"

                            };
                            client.SendResponse(response.ToJson());
                        }
                    }
                    else if (req.Method.ToLower() == "echo")
                    {
                            var response = new
                            {
                                Status = $"{ (int)StatusCode.Ok} Ok",
                                req.Body

                            };
                            client.SendResponse(response.ToJson());
                        
                    }


                }
            }
            else
            {
                if (res.ToArray().Length > 1) {
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Bad_Request} bad request {String.Join(", ", res.ToArray())}"
                    };
                    client.SendResponse(response.ToJson());
                }
                else
                {
                    var response = new
                    {
                        Status = $"{ (int)StatusCode.Bad_Request} bad request"
                    };
                    client.SendResponse(response.ToJson());
                }

                
            }

        }

        public static void SendResponse(this TcpClient client, string response)
        {
            var msg = Encoding.UTF8.GetBytes(response);
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
                    
                    test = (int)Reason.To_create;
                    break;
                case "read":
                  
                    test = (int)Reason.Ok;
                    break;
                case "update":
                  
                    test = (int)Reason.To_update;
                    break;
                case "delete":
                  
                    test = (int)Reason.Ok;
                    break;

                case "echo":
                   
                    test = (int)Reason.Ok;
                    break;
                case "":
                  
                    test = (int)Reason.Missing;
                    break;
                default:
                   
                    test = (int)Reason.Illegal;
                    break;
            }
            return test;
        }


        public static int VerifyPath(Request req)
        {
            int test;
            var method = req.Method;
            Regex case_1 = new Regex(@"^/api/categories$");
            Regex case_2 = new Regex(@"^/api/categories/\d+$");
            Regex case_3 = new Regex(@"^/api/categories(/\d+)?$");
            Regex pattern = new Regex("");
            if (req.Method.ToLower() == "create")
            {
                pattern = case_1;
            }
            else if (req.Method.ToLower() == "delete")
            {
                pattern = case_2;
            }
            else if(req.Method.ToLower() == "update")
            {
                pattern = case_2;
            }
            else if(req.Method.ToLower() == "read")
            {
                
                pattern = case_3;
            }
            if (string.IsNullOrWhiteSpace(req.Path))
            {
                if (req.Method.ToLower() == "echo")
                {
                    test = (int)Reason.Ok;
                    return test;
                }
                else
                {
                    test = (int)Reason.Missing;
                    return test;
                }
            }
            else if (pattern.IsMatch(req.Path))
            {
                test = (int)Reason.Ok;
                return test;
            }
            else
            {
                test = (int)Reason.Illegal;
                return test;
            }


        }

        public static int VerifyDate(Request req)
        {
            bool success = long.TryParse(req.Date, out long result);
            int test;
            if (success)
            {
                Console.WriteLine("Converted '{0}' to {1}.", req.Date, result);
                test = (int)Reason.Ok;
                return test;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(req.Date))
                {
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
            

            if (req.Method.ToLower() == "read")
            {
                
                if (string.IsNullOrWhiteSpace(req.Body)) 
                { 
                    test = (int)Reason.Ok;
                   
                }
                else
                {

                    test = (int)Reason.Illegal;

                }
                 
                return test;
            }
            else if (req.Method.ToLower() == "delete")
            {

                if (string.IsNullOrWhiteSpace(req.Body)) test = (int)Reason.Ok;
                else
                {

                    test = (int)Reason.Illegal;

                }
                return test;
            }
            else if (req.Method.ToLower() == "update")
            {
                
                if (string.IsNullOrWhiteSpace(req.Body))
                {
                    test = (int)Reason.Missing;
                    return test;
                }
                else
                {
                     if(IsValidJson(req.Body) == 0)
                    {
                        var newEl = JsonSerializer.Deserialize<Category>(req.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        if (newEl.Id <= 0) test = (int)Reason.Illegal;
                        else return test = (int)Reason.Ok;
                    }
                    
                    else test = IsValidJson(req.Body);
                }
                return test;

            }
            else if (req.Method.ToLower() == "create")
            {
                if (string.IsNullOrWhiteSpace(req.Body))
                {
                    test = (int)Reason.Missing;
                    return test;
                }
                else
                {
                    var newEl = JsonSerializer.Deserialize<Category>(req.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    if (newEl.Id > 0) test = (int)Reason.Illegal;
                    else test = IsValidJson(req.Body);
                }
                return test;

            }
            else if (req.Method.ToLower() == "echo")
            {
                
                if (string.IsNullOrWhiteSpace(req.Body))
                {
                    return (int)Reason.Missing;
                }
                else
                {
                   var body = req.Body.Trim();
                    if ((body.StartsWith("{") && body.EndsWith("}")) ||
                        (body.StartsWith("[") && body.EndsWith("]")))
                    {
             
                        return (int)Reason.Illegal;
                    }
                    else
                    {
                        return (int)Reason.Ok;
                    }
                   
                }
            

            }
            else
            {
                return -1;
            }

        }
        // Utils

        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static int IsValidJson(string body)
        {
            int test;
            //Removes all leading and trailing white-space characters from the current string
            body = body.Trim();
            if ((body.StartsWith("{") && body.EndsWith("}")) || //For object
                (body.StartsWith("[") && body.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(body);
                    test = (int)Reason.Ok;
                    return test;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    test = (int)Reason.Illegal;
                    return test;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
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

        // Manage dictionary
        static void Insert(this Dictionary<int, string> category, string value)
        {
            category.Add(category.Keys.Last()+1, value);
        }

        static bool Delete(this Dictionary<int, string> category, int key)
        {
            if (!category.ContainsKey(key)) return false;
            else category.Remove(key); Console.WriteLine($"Delete from key = {key}."); return true; 
        }

        static string FindById(this Dictionary<int, string> category, int key)
        {
            string value;
            if (category.TryGetValue(key, out value)) Console.WriteLine($"For key = {key}, value = {value}.");
            else
            {
                Console.WriteLine($"Key = {key} is not found.");
                value = "";
            }
            return value;

        }

        static string [] GetAll(this Dictionary<int, string> category)
        {
            List<string> res = new List<string>();
            foreach (KeyValuePair<int, string> kvp in category)
            {
                var test = new 
                { 
                    cid= kvp.Key, 
                    name= kvp.Value 
                };
            
                res.Add(test.ToJson());
           
            }
            
            return res.ToArray();

        }

        static bool Update(this Dictionary<int, string> category, int key, string value)
        {
            if (!category.ContainsKey(key)) return false;
            else 
            {
                category[key] = value;
                Console.WriteLine($"Update.");
                return true;
            }
        }


    }



}






