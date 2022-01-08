using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler {

    class HttpParser {

        private TcpClient _socket;
        private HttpRequestHandler _handler;
        private DbHandler _dbHandler;
        private bool _mvpset;

        private class User {
            public string username;
            public string password;
        }
        private User userinfo;                                              //body of request gets saved as an instance of User in case of login or registration


        public string Method { get; private set; }                          //request method (GET, POST, PUT, DELETE, ...)
        public string Version { get; private set; }                         //HTTP version
        public string Path { get; private set; }                            //URI path
        public string RequestContent { get; private set; }                  //attached content, i.e. JSON data

        public Dictionary<string, string> Headerparts { get; private set; }

        public HttpParser(TcpClient socket, HttpRequestHandler handler) {
            _socket = socket;
            _handler = handler;
            _mvpset = false;
            _dbHandler = new DbHandler();

            Headerparts = new Dictionary<string, string>();
        }

        private void Write(StreamWriter writer, string str) {
            //write to stream and output on console
            writer.WriteLine(str);
            Console.WriteLine(str);
        }

        private void Respond(StreamWriter writer) {
            //write a response based on the request
            HttpStatusCode status = HttpStatusCode.NotFound;  //placeholder for actual status response
            string content = $"<html><body><h1>Error {(int)status}</h1>Page not found</form></html>";
            switch (Method) {
                //depending on the method and returned status (201 for successful registration, 409 for already existing username, etc.) set status and response accordingly
                //combinations of methods, path, and response codes that are not used result in a "page not found" error

                /*
                    Possible responses:

                    HttpStatusCode.Created:
                        - user was successfully inserted into the database
                    HttpStatusCode.Conflict:
                        - user could not be registered because its username already exists in the database
                    HttpStatusCode.OK:
                        - user successfully logged in
                    HttpStatusCode.Unauthorized:
                        - user could not login because an incorrect username or password was given
                    HttpStatusCode.InternalServerError:
                        - the server ran into a problem when processing the input
                */

                case "GET":
                    switch (Path) {
                        case "/<placeholder>":
                            content = "<html><body><h1>Title</h1>Response</form></html>";
                            break;
                    }
                    break;

                case "POST":
                    switch (Path) {
                        case "/users":      //registration request
                            //check if header contains json content
                            if (Headerparts.ContainsKey("Content-Type")) {
                                if (Headerparts["Content-Type"] == "application/json") {
                                    //save user info into class
                                    userinfo = JsonConvert.DeserializeObject<User>(RequestContent);
                                }
                                else {
                                    Console.Error.WriteLine("Unexpected content type in header");
                                    break;
                                }
                            }
                            else {
                                Console.Error.WriteLine("Missing content in POST request");
                                break;
                            }
                            status = _dbHandler.RegisterUser(userinfo.username, userinfo.password);
                            switch (status) {
                                case HttpStatusCode.Created:
                                    content = "<html><body><h1>Registration successful</h1>You can now proceed to login</form></html>";
                                    break;

                                case HttpStatusCode.Conflict:
                                    content = $"<html><body><h1>Error {(int)status}</h1>Username already exists. Choose a different name</form></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred. Try again later</form></html>";
                                    break;
                            }
                            break;

                        case "/sessions":   //login request
                            //check if header contains json content
                            if (Headerparts.ContainsKey("Content-Type")) {
                                if (Headerparts["Content-Type"] == "application/json") {
                                    //save user info into class
                                    userinfo = JsonConvert.DeserializeObject<User>(RequestContent);
                                }
                                else {
                                    break;
                                }
                            }
                            else {
                                break;
                            }
                            status = _dbHandler.LoginUser(userinfo.username, userinfo.password);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "<html><body><h1>Login successful</h1>View all your profile information here</form></html>";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "<html><body><h1>Login failed</h1>Username or password wrong. Try again</form></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred. Try again later</form></html>";
                                    break;
                            }
                            break;
                    }
                    break;

                case "PUT":
                    switch (Path) {
                        case "/<placeholder>":
                            content = "<html><body><h1>Title</h1>Response</form></html>";
                            break;
                    }
                    break;

                case "DELETE":
                    switch (Path) {
                        case "/<placeholder>":
                            content = "<html><body><h1>Title</h1>Response</form></html>";
                            break;
                    }
                    break;
            }

            Console.WriteLine();
            Write(writer, $"HTTP/1.1 {(int)status} {status}");
            Write(writer, "Server: My simple HttpServer");
            Write(writer, $"Current Time: {DateTime.Now}");
            Write(writer, $"Content-Length: {content.Length}");
            Write(writer, "Content-Type: text/html; charset=utf-8");
            Write(writer, "");
            Write(writer, content);
            writer.WriteLine();

            writer.Close();
        }

        public void Parse() {
            StreamWriter writer = new StreamWriter(_socket.GetStream());
            StreamReader reader = new StreamReader(_socket.GetStream());

            Console.WriteLine();

            string line;

            //read raw HTTP request information
            while((line = reader.ReadLine()) != null) {
                if(line.Length == 0) {
                    //end of header reached; request content (i.e. JSON) is next
                    break;
                }

                if(_mvpset == false) {
                    string[] parts = line.Split(' ');

                    Method = parts[0];
                    Path = parts[1];
                    Version = parts[2];

                    _mvpset = true;
                }
                else {
                    string[] parts = line.Split(": ");
                    Headerparts.Add(parts[0], parts[1]);
                }
            }

            try {
                if (Headerparts.ContainsKey("Content-Type")) {
                    //use content length in header to determine number of characters in the request body
                    int conlen = int.Parse(Headerparts["Content-Length"]);
                    //read number of characters into a buffer
                    char[] buff = new char[conlen];
                    reader.Read(buff, 0, conlen);
                    //create a string from the buffer containing the content and save it into the property RequestContent
                    string buffstring = new string(buff);
                    RequestContent = buffstring;
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }

            //print seperated HTTP request in console for easier debugging
            Console.WriteLine($"Method: {Method}");
            Console.WriteLine($"Path: {Path}");
            Console.WriteLine($"Version: {Version}");
            foreach (KeyValuePair<string, string> values in Headerparts) {
                Console.WriteLine($"{values.Key}: {values.Value}");
            }
            Console.WriteLine();
            Console.WriteLine($"Content:\n{RequestContent ?? "Null"}");

            Respond(writer);
        }
    }
}
