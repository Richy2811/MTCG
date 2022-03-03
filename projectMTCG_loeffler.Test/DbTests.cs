using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler.Test {
    public class DbTests {
        public void User_Registration_Return_Status_Created() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/users"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "56"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.RegisterUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Created, status);
        }

        public void User_Registration_Duplicate_Return_Status_Conflict() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/users"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "56"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.RegisterUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Conflict, status);
        }

        public void User_Login_Return_Status_OK() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "56"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.AuthenticateUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.OK, status);
        }

        public void User_Login_Wrong_Username_Return_Status_Unauthorized() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testusr\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "55"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.AuthenticateUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        public void User_Login_Wrong_Password_Return_Status_Unauthorized() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\",\"Password\": \"password2\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "56"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.AuthenticateUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        public void User_Delete_No_Token_Return_Status_Unauthorized() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "28"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.DeleteUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        public void User_Delete_Bad_Token_Return_Status_Forbidden() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Authorization", "Basic testuser-mtcgToken"},
                {"Content-Length", "28"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.DeleteUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Forbidden, status);
        }

        public void User_Delete_Admin_Token_Return_Status_OK() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Authorization", "Basic admin-mtcgToken"},
                {"Content-Length", "28"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.DeleteUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.OK, status);
        }

        public void User_Delete_Admin_Token_Return_Status_NotFound() {
            //arrange
            string userjsonstring = "{\"Username\": \"Testuser\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Authorization", "Basic admin-mtcgToken"},
                {"Content-Length", "28"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.DeleteUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.NotFound, status);
        }

        [Test]
        public void User_Test_Sequence() {
            User_Registration_Return_Status_Created();
            User_Registration_Duplicate_Return_Status_Conflict();

            User_Login_Return_Status_OK();
            User_Login_Wrong_Username_Return_Status_Unauthorized();
            User_Login_Wrong_Password_Return_Status_Unauthorized();

            User_Delete_No_Token_Return_Status_Unauthorized();
            User_Delete_Bad_Token_Return_Status_Forbidden();
            User_Delete_Admin_Token_Return_Status_OK();
            User_Delete_Admin_Token_Return_Status_NotFound();
        }
    }
}