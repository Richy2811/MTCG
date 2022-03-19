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


        private string BytesToHex(byte[] hashbytes) {
            StringBuilder returnStr = new StringBuilder(64);
            for (int i = 0; i < hashbytes.Length; i++) {
                returnStr.Append(hashbytes[i].ToString("x2"));
            }
            return returnStr.ToString();
        }


        private HttpStatusCode VerifyPassword(string username, string password) {   //check if this username - password pair matches yields a match in database; HttpStatusCode.OK if match was found
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
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, username);
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
                    byte[] hashStr = Encoding.UTF8.GetBytes(password);
                    byte[] hashValue = mySha256.ComputeHash(hashStr);
                    string hashValueStr = BytesToHex(hashValue);

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


        public string GetStats(string authorization) {
            string[] auth = authorization.Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(auth[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

            string selectPassword = "SELECT coins, elo FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return "Error";
            }

            NpgsqlCommand command = new NpgsqlCommand(selectPassword, conn);
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userinfo[0]);
            command.Prepare();
            NpgsqlDataReader queryreader = command.ExecuteReader();
            if (queryreader.Read()) {   //there should only be one result
                return $"{{Username: {userinfo[0]}, Coins: {queryreader[0]}, ELO: {queryreader[1]}}}";
            }
            else {
                return "An error occurred";
            }
        }


        public HttpStatusCode Stats(Dictionary<string, string> headerParts) {
            if (!headerParts.ContainsKey("Authorization")) {
                return HttpStatusCode.Unauthorized;
            }

            string[] authorization = headerParts["Authorization"].Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

            return VerifyPassword(userinfo[0], userinfo[1]);
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
            byte[] hashStr = Encoding.UTF8.GetBytes(userObject["Password"].ToString());
            byte[] hashedPassword = mySha256.ComputeHash(hashStr);
            string hashedPasswordStr = BytesToHex(hashedPassword);

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
            command.Parameters.AddWithValue("passw", NpgsqlDbType.Varchar, 64, hashedPasswordStr);
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

            return VerifyPassword(userObject["Username"].ToString(), userObject["Password"].ToString());
        }


        public HttpStatusCode AddPackage(string packageJsonString, Dictionary<string, string> headerParts) {
            //check admin token
            if (headerParts.ContainsKey("Authorization")) {
                string[] authorization = headerParts["Authorization"].Split(" ");
                byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
                string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
                string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

                if (VerifyPassword(userinfo[0], userinfo[1]) != HttpStatusCode.OK) {
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
            if (!headerParts.ContainsKey("Authorization")) {
                return HttpStatusCode.Forbidden;
            }
            JObject wantedPackId;

            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    wantedPackId = JObject.Parse(packageJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /transactions/packages");
                return HttpStatusCode.UnprocessableEntity;
            }

            string[] authorization = headerParts["Authorization"].Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

            if (VerifyPassword(userinfo[0], userinfo[1]) == HttpStatusCode.OK) {
                //token ok, add package with given id in json string if coins are sufficient
                int coins = 0;
                int price = Int32.MaxValue;
                JArray userStack = null;
                JArray cardPackage = null;
                bool nullStack = false;

                string selectUser = "SELECT coins, cardstack FROM users WHERE username = @uname";
                string selectPackage = "SELECT cardpackage, price FROM market WHERE id = @id";
                NpgsqlConnection conn = new NpgsqlConnection(_connString);
                try {
                    conn.Open();
                }
                catch (Exception e) {
                    Console.WriteLine($"Error {e.Message}");
                    return HttpStatusCode.InternalServerError;
                }

                NpgsqlCommand coinsCommand = new NpgsqlCommand(selectUser, conn);
                NpgsqlCommand packageCommand = new NpgsqlCommand(selectPackage, conn);

                coinsCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userinfo[0]);
                packageCommand.Parameters.AddWithValue("id", NpgsqlDbType.Integer, wantedPackId["Id"].Value<int>());

                coinsCommand.Prepare();
                packageCommand.Prepare();

                NpgsqlDataReader queryreader = coinsCommand.ExecuteReader();
                if (queryreader.Read()) {
                    //first result is number of coins the user has
                    coins = (int)queryreader[0];
                    //second relult is the cardstack of the user -> check if column result is null
                    if (!DBNull.Value.Equals(queryreader[1])) {
                        //user already has cards -> parse user stack and add cardpack to existing stack; set flag to signify existing stack
                        userStack = JArray.Parse(queryreader[1].ToString());
                        nullStack = false;
                    }
                    else {
                        //user has no cards yet -> insert cards directly into stack; setting flag to signify empty stack
                        nullStack = true;
                    }
                }
                else {
                    return HttpStatusCode.InternalServerError;
                }

                queryreader.Close();
                queryreader = packageCommand.ExecuteReader();
                if (queryreader.Read()) {
                    cardPackage = JArray.Parse(queryreader[0].ToString());
                    price = (int)queryreader[1];
                }
                else {
                    return HttpStatusCode.InternalServerError;
                }
                queryreader.Close();

                Console.WriteLine($"Coins: {coins}");
                Console.WriteLine($"Price: {price}");
                Console.WriteLine($"Package: {cardPackage}");
                Console.WriteLine($"CardStack: {userStack}");
                //todo
                //handle coins, price and cardpackage
                if ((coins - price) >= 0) {
                    //user has enough coins to buy package -> update user coin value -> add cards to their stack
                    if (!nullStack) {
                        //add cards from package to userstack
                        foreach (JToken card in cardPackage) {
                            userStack.Add(card);
                        }
                    }
                    else {
                        //assign package to userstack directly
                        userStack = cardPackage;
                    }

                    string removeFromMarket = "DELETE FROM market WHERE id = @removeid";
                    NpgsqlCommand deleteCommand = new NpgsqlCommand(removeFromMarket, conn);
                    deleteCommand.Parameters.AddWithValue("removeid", NpgsqlDbType.Integer, wantedPackId["Id"].Value<int>());
                    deleteCommand.Prepare();

                    string updateCoins = "UPDATE users SET coins = @newcoins, cardstack = @newcardstack WHERE username = @uname";
                    NpgsqlCommand updateCommand = new NpgsqlCommand(updateCoins, conn);
                    updateCommand.Parameters.AddWithValue("newcoins", NpgsqlDbType.Integer, coins - price);
                    updateCommand.Parameters.AddWithValue("newcardstack", NpgsqlDbType.Jsonb, userStack.ToString());
                    updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 50, userinfo[0]);
                    updateCommand.Prepare();

                    try {
                        //if one row of each table was affected the update was successful
                        if ((updateCommand.ExecuteNonQuery() == 1) && (deleteCommand.ExecuteNonQuery() == 1)) {
                            Console.WriteLine("Package successfully acquired");
                            conn.Close();
                            return HttpStatusCode.OK;
                        }
                        else {
                            Console.WriteLine("Error, something went wrong when updating database");
                            conn.Close();
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                    catch (NpgsqlException e) {
                        Console.Error.WriteLine($"Error {e.Message}");
                        conn.Close();
                        return HttpStatusCode.InternalServerError;
                    }
                }
                else {
                    //not enough coins
                    return HttpStatusCode.BadRequest;
                }
            }
            else {
                return VerifyPassword(userinfo[0], userinfo[1]);
            }
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
            //check admin token
            if (headerParts.ContainsKey("Authorization")) {
                string[] authorization = headerParts["Authorization"].Split(" ");
                byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
                string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
                string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

                if (VerifyPassword(userinfo[0], userinfo[1]) != HttpStatusCode.OK) {
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
