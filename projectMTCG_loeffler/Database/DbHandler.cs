using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace projectMTCG_loeffler.Database {
    public class DbHandler {
        private const string _connString = "Server=127.0.0.1; Port=5432; User Id=richy; Password=1234; Database=mtcgdb";
        private string _resultString;   //used to save a query result into a string

        #region GET Requests

        public HttpStatusCode ShowStack(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode ShowDeck(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode ShowDeckPlain(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode EditUser(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode ShowStats(string userJsonString, Dictionary<string, string> headerParts) {
            /*
            if (!headerParts.ContainsKey("Authorization")) {
                return HttpStatusCode.Unauthorized;
            }

            string[] authorization = headerParts["Authorization"].Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":");
            */
            //todo
            return HttpStatusCode.OK;
        }


        public HttpStatusCode ShowScores(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode ShowTrades(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }

        #endregion

        #region POST requests

        public HttpStatusCode RegisterUser(string userJsonString, Dictionary<string, string> headerParts) {
            JObject userObject;
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    userObject = JObject.Parse(userJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.InternalServerError;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /users");
                return HttpStatusCode.Unauthorized;
            }

            SHA256 mySha256 = SHA256.Create();
            byte[] hashstr = Encoding.UTF8.GetBytes(userObject["Password"].ToString());
            byte[] hashedPassword = mySha256.ComputeHash(hashstr);

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            string insertUser = "INSERT INTO users (username, password, coins, elo) VALUES (@uname, @passw, 20, 100)";
            NpgsqlCommand command = new NpgsqlCommand(insertUser, conn);
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userObject["Username"].ToString());
            command.Parameters.AddWithValue("passw", NpgsqlDbType.Varchar, 40, Encoding.UTF8.GetString(hashedPassword));
            command.Prepare();

            try {
                if (command.ExecuteNonQuery() == 1) {
                    Console.WriteLine("Registration successful");
                    conn.Close();
                    return HttpStatusCode.Created;
                }
                else {
                    Console.WriteLine("Registration failed");
                    conn.Close();
                    return HttpStatusCode.Conflict;
                }
            }
            catch (NpgsqlException e) {
                Console.Error.WriteLine($"Error {e.Message}");
                conn.Close();
                if (e.Message.Contains("unique constraint")) {
                    return HttpStatusCode.Conflict;
                }
                else {
                    return HttpStatusCode.InternalServerError;
                }
            }
        }


        public HttpStatusCode AuthenticateUser(string userJsonString, Dictionary<string, string> headerParts) {
            JObject userObject;
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    userObject = JObject.Parse(userJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.Unauthorized;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /users");
                return HttpStatusCode.Unauthorized;
            }
            SHA256 mySha256 = SHA256.Create();

            string selectPassword = "SELECT password FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            NpgsqlCommand command = new NpgsqlCommand(selectPassword, conn);

            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userObject["Username"].ToString());

            command.Prepare();

            NpgsqlDataReader queryreader = command.ExecuteReader();
            int results = 0;
            if (queryreader.Read()) {       //check if sql query contains at least one result
                results++;
                _resultString = queryreader[0].ToString();

            }

            while (queryreader.Read()) {    //check if sql query contains multiple results (should never happen because of the username unique constraint)
                results++;
            }
            

            switch (results) {
                case 0:
                    //no match could be found in db
                    conn.Close();
                    return HttpStatusCode.Unauthorized;

                case 1:
                    //matching username found in db
                    byte[] hashstr = Encoding.UTF8.GetBytes(userObject["Password"].ToString());
                    byte[] hashValue = mySha256.ComputeHash(hashstr);
                    string hashValueStr = Encoding.UTF8.GetString(hashValue);

                    conn.Close();
                    //compare hashstrings to each other
                    if (hashValueStr != _resultString) {
                        return HttpStatusCode.Unauthorized;
                    }
                    else {
                        return HttpStatusCode.OK;
                    }

                default:
                    //should never happen since usernames are unique in db
                    Console.Error.WriteLine("Error: Query returned multiple rows. Expected 1 or 0 results");
                    conn.Close();
                    return HttpStatusCode.InternalServerError;
            }
        }


        public HttpStatusCode AddPackage(string packageJsonString, Dictionary<string, string> headerParts) {
            //check for admin token
            if (headerParts.ContainsKey("Authorization")) {
                if (headerParts["Authorization"] == "Basic admin-mtcgToken") { }
                else {
                    return HttpStatusCode.Forbidden;
                }
            }
            else {
                return HttpStatusCode.Unauthorized;
            }

            JArray jsonArray;

            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    jsonArray = JArray.Parse(packageJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /packages");
                return HttpStatusCode.UnprocessableEntity;
            }

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            string insertUser = "INSERT INTO market (cardpackage, price) VALUES (@package, 5)";
            NpgsqlCommand command = new NpgsqlCommand(insertUser, conn);
            command.Parameters.AddWithValue("package", NpgsqlDbType.Jsonb, jsonArray.ToString());
            command.Prepare();

            try {
                if (command.ExecuteNonQuery() == 1) {
                    Console.WriteLine("Card package successfully added");
                    conn.Close();
                    return HttpStatusCode.Created;
                }
                else {
                    Console.WriteLine("Failed to add card package");
                    conn.Close();
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            catch (NpgsqlException e) {
                Console.Error.WriteLine($"Error {e.Message}");
                conn.Close();
                return HttpStatusCode.InternalServerError;
            }
        }


        public HttpStatusCode AquirePackage(string packageJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode StartBattle(string packageJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode CreateTrade(string packageJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }


        public HttpStatusCode AcceptTrade(string packageJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }

        #endregion

        #region PUT requests

        public HttpStatusCode ConfigDeck(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }

        #endregion

        #region DELETE requests

        public HttpStatusCode DeleteUser(string userJsonString, Dictionary<string, string> headerParts) {
            //check for admin token
            if (headerParts.ContainsKey("Authorization")) {
                if (headerParts["Authorization"] == "Basic admin-mtcgToken") {}
                else {
                    return HttpStatusCode.Forbidden;
                }
            }
            else {
                return HttpStatusCode.Unauthorized;
            }

            JObject userObject;
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    userObject = JObject.Parse(userJsonString);
                    //return error if json string does not contain username
                    if (!userObject.ContainsKey("Username")) {
                        return HttpStatusCode.UnprocessableEntity;
                    }
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in DELETE request /users");
                return HttpStatusCode.UnprocessableEntity;
            }

            string deleteUser = "DELETE FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            NpgsqlCommand command = new NpgsqlCommand(deleteUser, conn);

            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userObject["Username"].ToString());

            command.Prepare();

            try {
                if (command.ExecuteNonQuery() == 1) {
                    Console.WriteLine("User successfully deleted from database");
                    conn.Close();
                    return HttpStatusCode.OK;
                }
                else {
                    Console.WriteLine("Deletion failed");
                    conn.Close();
                    return HttpStatusCode.NotFound;
                }
            }
            catch (NpgsqlException e) {
                Console.Error.WriteLine($"Error {e.Message}");
                conn.Close();
                return HttpStatusCode.InternalServerError;
            }
        }


        public HttpStatusCode DeleteTrade(string userJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }

        #endregion
    }
}
