using System.Collections.Generic;
using System.Net;
using Moq;
using Npgsql;
using NUnit.Framework;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler.Test {
    public class Tests {
        [Test]
        public void User_Registration_Return_Status_Created() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richy\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/users"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.RegisterUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Created, status);
        }

        [Test]
        public void User_Registration_Duplicate_Return_Status_Conflict() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richy\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/users"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.RegisterUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Conflict, status);
        }

        /*[Test]
        public void User_Registration_Return_Status_Created() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richy\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/users"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            //DbHandler handler = new DbHandler();
            var dbHandlerMock = new Mock<DbHandler>();
            

            //act
            HttpStatusCode status = handler.RegisterUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Created, status);
        }*/

        [Test]
        public void User_Login_Return_Status_OK() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richy\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.LoginUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.OK, status);
        }

        [Test]
        public void User_Login_Wrong_Username_Return_Status_Unauthorized() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richi\",\"Password\": \"password1\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.LoginUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        [Test]
        public void User_Login_Wrong_Password_Return_Status_Unauthorized() {
            //arrange
            string userjsonstring = "{\"Username\": \"Richy\",\"Password\": \"password2\"}";
            Dictionary<string, string> headerParts = new Dictionary<string, string>() {
                {"Method", "Post"},
                {"Path", "/sessions"},
                {"Version", "HTTP/1.1"},
                {"Host", "localhost:2811"},
                {"Content-Type", "application/json"},
                {"Content-Length", "53"}
            };
            DbHandler handler = new DbHandler();

            //act
            HttpStatusCode status = handler.LoginUser(userjsonstring, headerParts);

            //assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }
    }
}