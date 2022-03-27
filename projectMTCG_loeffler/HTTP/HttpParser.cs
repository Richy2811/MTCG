using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using projectMTCG_loeffler.Battle;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler {

    class HttpParser {

        private TcpClient _socket;
        private HttpRequestHandler _handler;
        private DbHandler _dbHandler;
        private bool _mvpset;                                               //method version path flag
        private BattleHandler _battleHandler;


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
                        case "/stack":                  //show all cards currently in the stack
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.GetCards(Headerparts, true)?.ToString() ?? "An error occurred when reading from the database. Try again later";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case "/deck":                   //show all cards currently in the user's deck
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.GetCards(Headerparts, false)?.ToString() ?? "An error occurred when reading from the database. Try again later";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case "/deck?format=plain":      //show all cards currently in the user's deck in a different format
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.ShowDeckPlain(Headerparts);
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case var namepath when new Regex("^/users/\\w+$").IsMatch(namepath):    //show user data (by username i.e. "/users/Richy")
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.ShowUser(Headerparts, Path);
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case "/stats":                  //show user statistics
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.GetStats(Headerparts);
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case "/score":                  //show scoreboard
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = _dbHandler.ShowScores();
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
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
                                    content = "Registration successful. You can now proceed to login";
                                    break;

                                case HttpStatusCode.Conflict:
                                    content = "Error. Username already exists. Choose a different name";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error. An internal server error occurred. Try again later";
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
                                    content = "Login successful";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Login failed. Username or password wrong. Try again";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error. An internal server error occurred. Try again later";
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
                                    content = "Insert successful. Card package was added successfully";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized. Authorization is required in order to carry out this request";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden. Insufficient rights to carry out this request";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "Missing content. Request is missing content or received wrong content type";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error. An internal server error occurred. Try again later";
                                    break;
                            }
                            break;

                        case "/transactions/packages":  //aquire a package from the market
                            status = _dbHandler.AquirePackage(RequestContent, Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "Package successfully acquired";
                                    break;

                                case HttpStatusCode.BadRequest:
                                    content = "Request failed, not enough coins";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Request failed, no authorization header";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Request failed, authentication invalid";
                                    break;

                                case HttpStatusCode.Gone:
                                    content = "Request failed, card pack is no longer available";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "Request failed, not enough coins";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case "/battles":                //start a battle with another user
                            status = _dbHandler.CheckToken(Headerparts);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    try {
                                        _battleHandler = new BattleHandler(RequestContent, Headerparts, _dbHandler);
                                        content = _battleHandler.StartBattle();
                                    }
                                    catch (Exception e) {
                                        Console.Error.WriteLine($"Error, {e.Message}");
                                    }
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
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
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "Successfully configured deck";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "Could not carry out task. Header information missing";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.BadRequest:
                                    content = "Could not carry out task. Four acquired cards have to be chosen to be in the deck";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
                            break;

                        case var namepath when new Regex("^/users/\\w+$").IsMatch(namepath):    //edit user data (by username i.e. "/users/Richy")
                            status = _dbHandler.EditUser(RequestContent, Headerparts, Path);
                            switch (status) {
                                case HttpStatusCode.OK:
                                    content = "Successfully changed user data";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "Could not carry out task. Header information missing";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Forbidden";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error";
                                    break;
                            }
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
                                    content = "Deletion successful. Resource successfully deleted";
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    content = "Unauthorized. Authorization is required in order to carry out this request";
                                    break;

                                case HttpStatusCode.Forbidden:
                                    content = "Insufficient rights to carry out this request";
                                    break;

                                case HttpStatusCode.UnprocessableEntity:
                                    content = "Missing content. Request is missing content or received wrong content type";
                                    break;

                                case HttpStatusCode.NotFound:
                                    content = "Error. Username could not be found. Deletion of user failed";
                                    break;

                                case HttpStatusCode.InternalServerError:
                                    content = "Error. An internal server error occurred when attempting to carry out the request";
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
            if (content != null) {
                Write(writer, $"Content-Length: {content.Length}");
                Write(writer, "Content-Type: text/plain; charset=utf-8");
            }
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
