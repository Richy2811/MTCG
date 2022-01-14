using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace projectMTCG_loeffler.Database {
    public class DbHandler {
        private const string _connString = "Server=127.0.0.1; Port=5432; User Id=richy; Password=1234; Database=mtcgdb";
        private string _resultString;   //used to save a query result into a string

        public class User {
            public string username;
            public string password;
        }
        private User userinfo;                                              //body of request gets saved as an instance of User in case of login or registration

        public HttpStatusCode RegisterUser(string userJsonString, Dictionary<string, string> headerParts) {
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //save user info into class
                    userinfo = JsonConvert.DeserializeObject<User>(userJsonString);
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
            byte[] hashstr = Encoding.UTF8.GetBytes(userinfo.password);
            byte[] hashedPassword = mySha256.ComputeHash(hashstr);

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            string insertUser = "INSERT INTO users (username, password, coins) VALUES (@uname, @passw, 20)";
            NpgsqlCommand command = new NpgsqlCommand(insertUser, conn);
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userinfo.username);
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

        public HttpStatusCode LoginUser(string userJsonString, Dictionary<string, string> headerParts) {
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //save user info into class
                    userinfo = JsonConvert.DeserializeObject<User>(userJsonString);
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

            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userinfo.username);

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
                    byte[] hashstr = Encoding.UTF8.GetBytes(userinfo.password);
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

        public HttpStatusCode AddPackages(string userJsonString, Dictionary<string, string> headerParts) {
            //check the authentication token
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //save user info into class
                    userinfo = JsonConvert.DeserializeObject<User>(userJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.Unauthorized;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /packages");
                return HttpStatusCode.Unauthorized;
            }
            return HttpStatusCode.InternalServerError;
        }
    }
}
