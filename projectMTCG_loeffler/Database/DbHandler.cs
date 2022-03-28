using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using projectMTCG_loeffler.cards;

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


        private bool ValidateType(string typeCondition) {
            List<string> cardTypes = new List<string>();
            cardTypes.Add("Monster");
            cardTypes.Add("Spell");
            cardTypes.Add("Goblin");
            cardTypes.Add("Dragon");
            cardTypes.Add("Kraken");
            cardTypes.Add("Wizzard");
            cardTypes.Add("Knight");
            cardTypes.Add("FireElve");
            cardTypes.Add("Ork");

            foreach (string typeName in cardTypes) {
                if (typeCondition == typeName) {
                    return true;
                }
            }
            return false;
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
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
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


        public string BasicAuthGetUsername(string authHeader) {
            string[] authorization = authHeader.Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

            return userinfo[0];
        }


        private string BasicAuthGetPassword(string authHeader) {
            string[] authorization = authHeader.Split(" ");
            byte[] userinfoencoded = Convert.FromBase64String(authorization[1]);
            string userinfodecoded = Encoding.UTF8.GetString(userinfoencoded);
            string[] userinfo = userinfodecoded.Split(":"); //userinfo[0] is username; userinfo[1] is password

            return userinfo[1];
        }


        public HttpStatusCode CheckToken(Dictionary<string, string> headerParts) {
            if (!headerParts.ContainsKey("Authorization")) {
                return HttpStatusCode.Forbidden;
            }

            string username = BasicAuthGetUsername(headerParts["Authorization"]);
            string password = BasicAuthGetPassword(headerParts["Authorization"]);

            return VerifyPassword(username, password);
        }


        public decimal GetEloRating(string username) {
            string selectElo = "SELECT elo FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                throw new Exception("Could not establish connection to database");
            }

            NpgsqlCommand selectEloCommand = new NpgsqlCommand(selectElo, conn);
            selectEloCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
            selectEloCommand.Prepare();

            //get values from winner
            NpgsqlDataReader queryReader = selectEloCommand.ExecuteReader();
            if (queryReader.Read()) {
                return (decimal)queryReader[0];
            }
            else {
                throw new Exception("Could not retrieve data from database");
            }
        }


        public void UpdateElo(string winner, string loser) {
            int winningPlayerWins, losingPlayerWins, winningPlayerLosses, losingPlayerLosses;
            winningPlayerWins = losingPlayerWins = winningPlayerLosses = losingPlayerLosses = 0;
            decimal winningPlayerElo = 0, losingPlayerElo = 0;

            int kValue = 20;
            
            string selectStats = "SELECT wins, losses, elo FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return;
            }

            NpgsqlCommand selectStatsCommand = new NpgsqlCommand(selectStats, conn);
            selectStatsCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, winner);
            selectStatsCommand.Prepare();

            //get values from winner
            NpgsqlDataReader queryReader = selectStatsCommand.ExecuteReader();
            if (queryReader.Read()) {
                winningPlayerWins = (int)queryReader[0] + 1;
                winningPlayerLosses = (int)queryReader[1];
                winningPlayerElo = (decimal)queryReader[2];
            }
            queryReader.Close();
            selectStatsCommand.Unprepare();
            selectStatsCommand.Parameters.Clear();


            //reuse command for losing player
            selectStatsCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, loser);
            selectStatsCommand.Prepare();

            queryReader = selectStatsCommand.ExecuteReader();
            if (queryReader.Read()) {
                losingPlayerWins = (int)queryReader[0];
                losingPlayerLosses = (int)queryReader[1] + 1;
                losingPlayerElo = (decimal)queryReader[2];
            }
            queryReader.Close();
            selectStatsCommand.Unprepare();
            selectStatsCommand.Parameters.Clear();

            float winningPlayerFinalElo;
            float losingPlayerFinalElo;

            //calculate expected result change of winning player
            float expectedResult = 1 / (1 + MathF.Pow(10, ((float)winningPlayerElo - (float)losingPlayerElo) / 400));
            
            Console.WriteLine($"Expected: {expectedResult}");

            //calculate final result of both players
            winningPlayerFinalElo = (float)winningPlayerElo + kValue * expectedResult;
            losingPlayerFinalElo = (float)losingPlayerElo - kValue * expectedResult;

            Console.WriteLine(winningPlayerFinalElo);
            Console.WriteLine(losingPlayerFinalElo);

            string updateString = "UPDATE users SET wins = @newwins, losses = @newlosses, elo = @newelo WHERE username = @uname";
            NpgsqlCommand updateCommand = new NpgsqlCommand(updateString, conn);
            updateCommand.Parameters.AddWithValue("newwins", NpgsqlDbType.Integer, winningPlayerWins);
            updateCommand.Parameters.AddWithValue("newlosses", NpgsqlDbType.Integer, winningPlayerLosses);
            updateCommand.Parameters.AddWithValue("newelo", NpgsqlDbType.Numeric, (decimal)winningPlayerFinalElo);
            updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, winner);
            updateCommand.Prepare();

            if (updateCommand.ExecuteNonQuery() != 1) {
                Console.Error.WriteLine("Something went wrong when updating player statistics");
                conn.Close();
                return;
            }
            updateCommand.Unprepare();
            updateCommand.Parameters.Clear();


            updateCommand.Parameters.AddWithValue("newwins", NpgsqlDbType.Integer, losingPlayerWins);
            updateCommand.Parameters.AddWithValue("newlosses", NpgsqlDbType.Integer, losingPlayerLosses);
            updateCommand.Parameters.AddWithValue("newelo", NpgsqlDbType.Numeric, (decimal)losingPlayerFinalElo);
            updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, loser);
            updateCommand.Prepare();
            
            if (updateCommand.ExecuteNonQuery() != 1) {
                Console.Error.WriteLine("Something went wrong when updating player statistics");
                conn.Close();
                return;
            }
        }


        #region GET Requests

        public JArray GetCards(Dictionary<string, string> headerParts, bool getStack) {
            string username = BasicAuthGetUsername(headerParts["Authorization"]);

            JArray cardStack = JArray.Parse("[]");

            string selectStack;
            //if flag is set to true then return stack of user; else return deck of user
            selectStack = getStack ? "SELECT cardstack FROM users WHERE username = @uname" : "SELECT carddeck FROM users WHERE username = @uname";

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                //return null if an error occured
                return null;
            }

            NpgsqlCommand selectStackCommand = new NpgsqlCommand(selectStack, conn);
            selectStackCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
            selectStackCommand.Prepare();
            NpgsqlDataReader queryreader = selectStackCommand.ExecuteReader();
            if (queryreader.Read()) {   //there should only be one result
                if (!DBNull.Value.Equals(queryreader[0])) {
                    cardStack = JArray.Parse(queryreader[0].ToString());
                }
                conn.Close();
                return cardStack;
            }
            else {
                conn.Close();
                return null;
            }
        }


        public JArray GetCards(string username, bool getStack) {
            JArray cardStack = JArray.Parse("[]");

            string selectStack;
            //if flag is set to true then return stack of user; else return deck of user
            selectStack = getStack ? "SELECT cardstack FROM users WHERE username = @uname" : "SELECT carddeck FROM users WHERE username = @uname";

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                //return null if an error occured
                return null;
            }

            NpgsqlCommand selectStackCommand = new NpgsqlCommand(selectStack, conn);
            selectStackCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
            selectStackCommand.Prepare();
            NpgsqlDataReader queryreader = selectStackCommand.ExecuteReader();
            if (queryreader.Read()) {   //there should only be one result
                if (!DBNull.Value.Equals(queryreader[0])) {
                    cardStack = JArray.Parse(queryreader[0].ToString());
                }
                conn.Close();
                return cardStack;
            }
            else {
                conn.Close();
                return null;
            }
        }


        public string ShowDeckPlain(Dictionary<string, string> headerParts) {
            JArray userDeck = GetCards(headerParts, false);
            StringBuilder userDeckPlain = new StringBuilder();

            int i = 1;
            Dictionary<string, string> deserialCards;
            foreach (JObject card in userDeck) {
                userDeckPlain.AppendLine("Card " + i);
                deserialCards = JsonConvert.DeserializeObject<Dictionary<string, string>>(card.ToString());
                foreach (var kvPair in deserialCards) {
                    userDeckPlain.AppendLine(kvPair.Key + ": " + kvPair.Value);
                }
                userDeckPlain.AppendLine();
                i++;
            }

            if (userDeckPlain.Length == 0) {
                return "Empty";
            }
            else {
                return userDeckPlain.ToString();
            }
        }


        public string ShowUser(Dictionary<string, string> headerParts, string path) {
            string pathUsername = path.Split("/users/")[1];
            string username = BasicAuthGetUsername(headerParts["Authorization"]);

            if (pathUsername == username) {
                string selectUserData = "SELECT username, status, country FROM users WHERE username = @uname";
                NpgsqlConnection conn = new NpgsqlConnection(_connString);
                try {
                    conn.Open();
                }
                catch (Exception e) {
                    Console.WriteLine($"Error {e.Message}");
                    return "Error";
                }

                NpgsqlCommand userDataCommand = new NpgsqlCommand(selectUserData, conn);
                userDataCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, pathUsername);
                userDataCommand.Prepare();
                NpgsqlDataReader queryReader = userDataCommand.ExecuteReader();
                if (queryReader.Read()) {
                    JObject userData = JObject.Parse($"{{Username: \"{queryReader[0]}\", Status: \"{queryReader[1]}\", Country: \"{queryReader[2]}\"}}");
                    conn.Close();
                    return userData.ToString();
                }
                else {
                    return "Error";
                }
            }
            else {
                return "Unauthorized";
            }
        }


        public string GetStats(Dictionary<string, string> headerParts) {
            string username = BasicAuthGetUsername(headerParts["Authorization"]);

            string selectPassword = "SELECT coins, wins, losses, elo FROM users WHERE username = @uname";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return "Error";
            }

            NpgsqlCommand command = new NpgsqlCommand(selectPassword, conn);
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
            command.Prepare();
            NpgsqlDataReader queryreader = command.ExecuteReader();
            if (queryreader.Read()) {   //there should only be one result
                //format decimal numbers so they use '.' as the separator in order for JObject.Parse() to work
                NumberFormatInfo format = new NumberFormatInfo();
                format.NumberDecimalSeparator = ".";
                decimal elo = (decimal)queryreader[3];
                float winLoseRatio = (float)(int)queryreader[1] / ((float)(int)queryreader[1] + (float)(int)queryreader[2]);
                //read into jobject to return indented json format
                JObject userStats = JObject.Parse($"{{Username: \"{username}\", Coins: {queryreader[0]}, Wins: {queryreader[1]}, Losses: {queryreader[2]}, \"Win/Lose Ratio\": {winLoseRatio.ToString(format)}, ELO: {elo.ToString(format)}}}");
                
                conn.Close();
                return userStats.ToString();
            }
            else {
                conn.Close();
                return "An error occurred";
            }
        }


        public string ShowScores() {
            string selectScores = "SELECT username, wins, losses, elo FROM users WHERE NOT username = 'Administrator' ORDER BY elo DESC";
            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return "Error";
            }
            NpgsqlCommand selectScoresCommand = new NpgsqlCommand(selectScores, conn);
            NpgsqlDataReader queryreader = selectScoresCommand.ExecuteReader();

            StringBuilder returnString = new StringBuilder();
            returnString.AppendLine($"|{String.Format("{0, 8}", "Rank")}|{String.Format("{0, 20}", "Username")}|{String.Format("{0, 10}", "Wins")}|{String.Format("{0, 10}", "Losses")}|{String.Format("{0, 10}", "W/L Ratio")}|{String.Format("{0, 12}", "ELO")}|");
            returnString.AppendLine("|--------+--------------------+----------+----------+----------+------------|");
            //get data set of all users excluding the administrator
            int i = 0;
            float winLoseRatio = 0;
            decimal previousElo = Decimal.MaxValue;
            while (queryreader.Read()) {
                //if previous player has the same elo value the placement gets shared
                if ((decimal)queryreader[3] != previousElo) {
                    i++;
                }
                previousElo = (decimal)queryreader[3];

                winLoseRatio = (float)(int)queryreader[1] / ((float)(int)queryreader[1] + (float)(int)queryreader[2]);
                returnString.AppendLine($"|{String.Format("{0, 8}", i)}|{String.Format("{0, 20}", queryreader[0])}|{String.Format("{0, 10}", queryreader[1])}|{String.Format("{0, 10}", queryreader[2])}|{String.Format("{0, 10}", winLoseRatio)}|{String.Format("{0, 12}", queryreader[3])}|");
            }
            conn.Close();
            return returnString.ToString();
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

            string insertUser = "INSERT INTO users (username, password, coins, wins, losses, elo) VALUES (@uname, @passw, 20, 0, 0, 1000)";
            NpgsqlCommand command = new NpgsqlCommand(insertUser, conn);
            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, userObject["Username"].ToString());
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
                string username = BasicAuthGetUsername(headerParts["Authorization"]);
                string password = BasicAuthGetPassword(headerParts["Authorization"]);

                if (VerifyPassword(username, password) != HttpStatusCode.OK) {
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

            string username = BasicAuthGetUsername(headerParts["Authorization"]);
            string password = BasicAuthGetPassword(headerParts["Authorization"]);

            if (VerifyPassword(username, password) == HttpStatusCode.OK) {
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

                coinsCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
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
                    conn.Close();
                    return HttpStatusCode.InternalServerError;
                }

                queryreader.Close();
                queryreader = packageCommand.ExecuteReader();
                if (queryreader.Read()) {
                    cardPackage = JArray.Parse(queryreader[0].ToString());
                    price = (int)queryreader[1];
                }
                else {
                    conn.Close();
                    return HttpStatusCode.Gone;
                }
                queryreader.Close();
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
                    updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
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
                    conn.Close();
                    return HttpStatusCode.BadRequest;
                }
            }
            else {
                return VerifyPassword(username, password);
            }
        }


        public HttpStatusCode CreateTrade(string tradeDetailsJsonString, Dictionary<string, string> headerParts) {
            //check authorization
            if (CheckToken(headerParts) != HttpStatusCode.OK) {
                return CheckToken(headerParts);
            }

            JObject tradeDetails;
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    tradeDetails = JObject.Parse(tradeDetailsJsonString);
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in POST request /transactions/tradings");
                return HttpStatusCode.UnprocessableEntity;
            }

            string username = BasicAuthGetUsername(headerParts["Authorization"]);
            string cardId = tradeDetails["CardId"].ToString();
            string typeCondition = null;
            int? minDamage = null;
            string elementCondition = null;

            if (tradeDetails.ContainsKey("TypeCondition")) {
                typeCondition = tradeDetails["TypeCondition"].ToString();

                //prevent bad or impossible conditions
                bool possibleType = ValidateType(typeCondition);
                if (!possibleType) {
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            if (tradeDetails.ContainsKey("MinDamageCondition")) {
                minDamage = (int)tradeDetails["MinDamageCondition"];

                //prevent bad or impossible conditions
                if (minDamage < 0) {
                    minDamage = 0;
                }
            }
            if (tradeDetails.ContainsKey("ElementCondition")) {
                elementCondition = tradeDetails["ElementCondition"].ToString();

                //prevent bad or impossible conditions
                bool possibleElement = false;
                foreach (Element elem in Enum.GetValues(typeof(Element))) {
                    if (elem.ToString() == elementCondition) {
                        possibleElement = true;
                    }
                }
                if (!possibleElement) {
                    return HttpStatusCode.UnprocessableEntity;
                }
            }

            JObject cardToTrade = null;
            JArray userStack = GetCards(headerParts, true);
            foreach (JObject card in userStack) {
                if (card["Id"].ToString() == cardId) {
                    cardToTrade = card;
                    userStack.Remove(card);
                    break;
                }
            }
            //if card does not appear in the user's stack the request fails
            if (cardToTrade == null) {
                return HttpStatusCode.BadRequest;
            }

            //first, select user id
            int userId = 0;
            string selectUid = "SELECT id FROM users WHERE username = @uname";

            NpgsqlConnection conn = new NpgsqlConnection(_connString);
            try {
                conn.Open();
            }
            catch (Exception e) {
                Console.WriteLine($"Error {e.Message}");
                return HttpStatusCode.InternalServerError;
            }

            NpgsqlCommand selectCommand = new NpgsqlCommand(selectUid, conn);

            selectCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);

            selectCommand.Prepare();
            NpgsqlDataReader queryReader = selectCommand.ExecuteReader();
            if (queryReader.Read()) {
                userId = (int)queryReader[0];
            }
            else {
                conn.Close();
                return HttpStatusCode.InternalServerError;
            }
            queryReader.Close();

            //update the user's card stack which now excludes the traded card
            string updateStack = "UPDATE users SET cardstack = @newcardstack WHERE username = @uname";
            NpgsqlCommand updateCommand = new NpgsqlCommand(updateStack, conn);
            updateCommand.Parameters.AddWithValue("newcardstack", NpgsqlDbType.Jsonb, userStack.ToString());
            updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
            updateCommand.Prepare();
            if (updateCommand.ExecuteNonQuery() != 1) {
                conn.Close();
                return HttpStatusCode.InternalServerError;
            }

            //insert trade deal into trading database
            string insertTrade = "INSERT INTO trades (userid, card, typecond, mindamagecond, elementcond) VALUES (@uid, @card, @typecond, @mindamagecond, @elementcond)";
            NpgsqlCommand insertCommand = new NpgsqlCommand(insertTrade, conn);

            insertCommand.Parameters.AddWithValue("@uid", NpgsqlDbType.Integer, userId);
            insertCommand.Parameters.AddWithValue("@card", NpgsqlDbType.Jsonb, cardToTrade.ToString());
            insertCommand.Parameters.AddWithValue("@typecond", NpgsqlDbType.Varchar, 10, !string.IsNullOrEmpty(typeCondition) ? typeCondition : DBNull.Value);
            insertCommand.Parameters.AddWithValue("@mindamagecond", NpgsqlDbType.Integer, (minDamage != null) ? minDamage : DBNull.Value);
            insertCommand.Parameters.AddWithValue("@elementcond", NpgsqlDbType.Varchar, 10, !string.IsNullOrEmpty(elementCondition) ? elementCondition : DBNull.Value);

            insertCommand.Prepare();
            if (insertCommand.ExecuteNonQuery() != 1) {
                return HttpStatusCode.InternalServerError;
            }
            return HttpStatusCode.OK;
        }


        public HttpStatusCode AcceptTrade(string packageJsonString, Dictionary<string, string> headerParts) {
            return HttpStatusCode.OK;
        }

        #endregion

        #region PUT requests

        public HttpStatusCode ConfigDeck(string idJsonString, Dictionary<string, string> headerParts) {
            if (!headerParts.ContainsKey("Authorization")) {
                return HttpStatusCode.Forbidden;
            }
            JArray idArray;
            //check if header contains json content
            if (headerParts.ContainsKey("Content-Type")) {
                if (headerParts["Content-Type"] == "application/json") {
                    //parse jsonstring
                    idArray = JArray.Parse(idJsonString);
                    if (idArray.Count != 4) {
                        return HttpStatusCode.BadRequest;
                    }
                }
                else {
                    Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                    return HttpStatusCode.UnprocessableEntity;
                }
            }
            else {
                Console.Error.WriteLine("Missing content in PUT request /deck");
                return HttpStatusCode.UnprocessableEntity;
            }

            string username = BasicAuthGetUsername(headerParts["Authorization"]);
            string password = BasicAuthGetPassword(headerParts["Authorization"]);

            if (VerifyPassword(username, password) == HttpStatusCode.OK) {
                JArray userStack = GetCards(headerParts, true);
                JArray userDeck = GetCards(headerParts, false);
                //first put all cards from the deck back into the stack
                foreach (JToken card in userDeck) {
                    userStack.Add(card);
                }
                userDeck.Clear();
                //then search for given id's
                foreach (JToken id in idArray) {
                    foreach (JObject card in userStack) {
                        if (card.GetValue("Id").ToString() == id.ToString()) {
                            userDeck.Add(card);
                            userStack.Remove(card);
                            break;
                        }
                    }
                }
                if (userDeck.Count != 4) {
                    return HttpStatusCode.BadRequest;
                }

                string updateStackAndDeck = "UPDATE users SET cardstack = @newstack, carddeck = @newdeck WHERE username = @uname";
                NpgsqlConnection conn = new NpgsqlConnection(_connString);
                try {
                    conn.Open();
                }
                catch (Exception e) {
                    Console.WriteLine($"Error {e.Message}");
                    return HttpStatusCode.InternalServerError;
                }
                NpgsqlCommand updateCommand = new NpgsqlCommand(updateStackAndDeck, conn);
                updateCommand.Parameters.AddWithValue("newstack", NpgsqlDbType.Jsonb, userStack.ToString());
                updateCommand.Parameters.AddWithValue("newdeck", NpgsqlDbType.Jsonb, userDeck.ToString());
                updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, username);
                updateCommand.Prepare();
                if (updateCommand.ExecuteNonQuery() == 1) {
                    conn.Close();
                    return HttpStatusCode.OK;
                }
                else {
                    conn.Close();
                    return HttpStatusCode.InternalServerError;
                }
            }
            else {
                return VerifyPassword(username, password);
            }
        }

        public HttpStatusCode EditUser(string userUpdateJsonString, Dictionary<string, string> headerParts, string path) {
            if (CheckToken(headerParts) == HttpStatusCode.OK) {
                string pathUsername = path.Split("/users/")[1];
                string username = BasicAuthGetUsername(headerParts["Authorization"]);
                JObject userUpdateData;
                //check if header contains json content
                if (headerParts.ContainsKey("Content-Type")) {
                    if (headerParts["Content-Type"] == "application/json") {
                        //parse jsonstring
                        userUpdateData = JObject.Parse(userUpdateJsonString);
                    }
                    else {
                        Console.Error.WriteLine("Unexpected content type in header. Expected <application/json>");
                        return HttpStatusCode.UnprocessableEntity;
                    }
                }
                else {
                    Console.Error.WriteLine($"Missing content in PUT request {path}");
                    return HttpStatusCode.UnprocessableEntity;
                }

                //check if the user actually tries to edit their own user data
                if (pathUsername == username) {
                    string updateUserData = "UPDATE users SET username = @newname, status = @newstatus, country = @newcountry WHERE username = @uname";
                    NpgsqlConnection conn = new NpgsqlConnection(_connString);
                    try {
                        conn.Open();
                    }
                    catch (Exception e) {
                        Console.WriteLine($"Error {e.Message}");
                        return HttpStatusCode.InternalServerError;
                    }
                    NpgsqlCommand updateCommand = new NpgsqlCommand(updateUserData, conn);
                    updateCommand.Parameters.AddWithValue("newname", NpgsqlDbType.Varchar, 20, userUpdateData["Username"].ToString());
                    updateCommand.Parameters.AddWithValue("newstatus", NpgsqlDbType.Varchar, 20, userUpdateData["Status"].ToString());
                    updateCommand.Parameters.AddWithValue("newcountry", NpgsqlDbType.Varchar, 20, userUpdateData["Country"].ToString());
                    updateCommand.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, pathUsername);
                    updateCommand.Prepare();
                    if (updateCommand.ExecuteNonQuery() == 1) {
                        conn.Close();
                        return HttpStatusCode.OK;
                    }
                    else {
                        conn.Close();
                        return HttpStatusCode.InternalServerError;
                    }
                }
                else {
                    return HttpStatusCode.Unauthorized;
                }
                
            }
            else {
                return CheckToken(headerParts);
            }
        }

        #endregion

        #region DELETE requests

        public HttpStatusCode DeleteUser(string userJsonString, Dictionary<string, string> headerParts) {
            //check admin token
            if (headerParts.ContainsKey("Authorization")) {
                string username = BasicAuthGetUsername(headerParts["Authorization"]);
                string password = BasicAuthGetPassword(headerParts["Authorization"]);

                if (VerifyPassword(username, password) != HttpStatusCode.OK) {
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

            command.Parameters.AddWithValue("uname", NpgsqlDbType.Varchar, 20, userObject["Username"].ToString());

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
