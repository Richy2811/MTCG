using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler {

    class HttpParser {

        private TcpClient _socket;
        private HttpRequestHandler _handler;
        private DbHandler _dbHandler;
        private bool _mvpset;


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
            string content = $"<html><body><h1>Error {(int)status}</h1>Page not found</body></html>";
            switch (Method) {
                //depending on the method and returned status (201 for successful registration, 409 for already existing username, etc.) set status and response accordingly
                //combinations of methods, path, and response codes that are not used result in a "page not found" response
                case "GET":
                    switch (Path) {
                        case "/cards":                  //show all aquired cards
                            status = _dbHandler.ShowStack(RequestContent, Headerparts);
                            break;

                        case "/deck":                   //show all cards currently in the user's deck
                            status = _dbHandler.ShowDeck(RequestContent, Headerparts);
                            break;

                        case "/deck?format=plain":      //show all cards currently in the user's deck in a different format
                            status = _dbHandler.ShowDeckPlain(RequestContent, Headerparts);
                            break;

                        case var namepath when new Regex("^/users/\\w+$").IsMatch(namepath):    //edit user data (by username i.e. "/users/richy")
                            status = _dbHandler.EditUser(RequestContent, Headerparts);
                            break;

                        case "/stats":                  //show user statistics
                            status = _dbHandler.ShowStats(RequestContent, Headerparts);
                            break;

                        case "/score":                  //show scoreboard
                            status = _dbHandler.ShowScores(RequestContent, Headerparts);
                            break;

                        case "/tradings":               //show trading deals
                            status = _dbHandler.ShowTrades(RequestContent, Headerparts);
                            break;
                    }
                    break;

                case "POST":
                    switch (Path) {
                        case "/users":                  //registration request
                            /*
                                Possible responses:

                                HttpStatusCode.Created:
                                    - user was successfully inserted into the database
                                HttpStatusCode.Conflict:
                                    - user could not be registered because its username already exists in the database
                                HttpStatusCode.InternalServerError:
                                    - the server ran into a problem when processing the input
                            */
                            status = _dbHandler.RegisterUser(RequestContent, Headerparts);
                            switch (status) {
                                case HttpStatusCode.Created:
                                    content = "<html><body><h1>Registration successful</h1>You can now proceed to login</body></html>";
                                    break;

                                case HttpStatusCode.Conflict:
                                    content = $"<html><body><h1>Error {(int)status}</h1>Username already exists. Choose a different name</body></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred. Try again later</body></html>";
                                    break;
                            }
                            break;

                        case "/sessions":               //login request
                            /*
                                Possible responses:

                                HttpStatusCode.OK:
                                    - user successfully logged in
                                HttpStatusCode.Unauthorized:
                                    - user could not login because an incorrect username or password was given
                                HttpStatusCode.InternalServerError:
                                    - the server ran into a problem when processing the input
                            */
                            status = _dbHandler.AuthenticateUser(RequestContent, Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "<html><body><h1>Login successful</h1>View all your profile information here</body></html>";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "<html><body><h1>Login failed</h1>Username or password wrong. Try again</body></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred. Try again later</body></html>";
                                    break;
                            }
                            break;

                        case "/packages":               //admin can add and regular user can acquire packages (request requires admin token)
                            /*
                                Possible responses:

                                HttpStatusCode.Created:
                                    - card package was successfully added to the market
                                HttpStatusCode.Unauthorized:
                                    - request fails due to the user having insufficient authorization
                                HttpStatusCode.Forbidden:
                                    - request fails due to the user not having the required permissions for this request (administration rights)
                                HttpStatusCode.UnprocessableEntity:
                                    - request fails due to missing content or wrong content type being submitted
                                HttpStatusCode.InternalServerError:
                                    - the server ran into a problem when processing the input
                            */
                            status = _dbHandler.AddPackage(RequestContent, Headerparts);
                            switch (status) {
                                case HttpStatusCode.Created:
                                    content = "<html><body><h1>Insert successful</h1>Card package was added successfully</body></html>";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "<html><body><h1>Unauthorized</h1>Authorization is required in order to carry out this request.</body></html>";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "<html><body><h1>Forbidden</h1>Insufficient rights to carry out this request.</body></html>";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "<html><body><h1>Missing content</h1>Request is missing content or received wrong content type.</body></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred. Try again later</body></html>";
                                    break;
                            }
                            break;

                        case "/transactions/packages":  //aquire a package from the market
                            status = _dbHandler.AquirePackage(RequestContent, Headerparts);
                            break;

                        case "/battles":                //start a battle with another user
                            status = _dbHandler.AquirePackage(RequestContent, Headerparts);
                            break;

                        case "/tradings":               //offer a trading deal
                            status = _dbHandler.CreateTrade(RequestContent, Headerparts);
                            break;

                        case var namepath when new Regex("^/tradings/[\\w|-]+$").IsMatch(namepath): //accept trading deal (by its id)
                            status = _dbHandler.AcceptTrade(RequestContent, Headerparts);
                            break;
                    }
                    break;

                case "PUT":
                    switch (Path) {
                        case "/deck":                   //configure deck
                            status = _dbHandler.ConfigDeck(RequestContent, Headerparts);
                            break;
                    }
                    break;

                case "DELETE":
                    switch (Path) {
                        case "/users":                  //delete user request (can only be done by admin through the admin token)
                            /*
                                Possible responses:
                            
                                HttpStatusCode.OK:
                                    - user was successfully removed from database
                                HttpStatusCode.Unauthorized:
                                    - request fails due to the user having insufficient authorization
                                HttpStatusCode.Forbidden:
                                    - request fails due to the user not having the required permissions for this request (administration rights)
                                HttpStatusCode.UnprocessableEntity:
                                    - request fails due to missing content or wrong content type being submitted
                                HttpStatusCode.NotFound:
                                    - request fails due to the provided username not being present in the database
                                HttpStatusCode.InternalServerError:
                                    - the server ran into a problem when processing the input
                            */
                            status = _dbHandler.DeleteUser(RequestContent, Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "<html><body><h1>Deletion successful</h1>Resource successfully deleted</body></html>";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "<html><body><h1>Unauthorized</h1>Authorization is required in order to carry out this request.</body></html>";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "<html><body><h1>Forbidden</h1>Insufficient rights to carry out this request.</body></html>";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "<html><body><h1>Missing content</h1>Request is missing content or received wrong content type.</body></html>";
                                    break;

                                case HttpStatusCode.NotFound:
                                    content = $"<html><body><h1>Error {(int)status}</h1>Username could not be found. Deletion of user failed.</body></html>";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = $"<html><body><h1>Error {(int)status}</h1>An internal server error occurred when attempting to carry out the request.</body></html>";
                                    break;
                            }
                            break;

                        case var namepath when new Regex("^/tradings/[\\w|-]+$").IsMatch(namepath): //delete trading deal (by its id)
                            status = _dbHandler.DeleteTrade(RequestContent, Headerparts);
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
                Console.Error.WriteLine($"Error {e.Message}");
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
