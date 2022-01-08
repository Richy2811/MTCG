using System;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler {
    class Program {
        static void Main(string[] args) {
            HttpRequestHandler handler = new HttpRequestHandler(2811);
            handler.Run();

            /*DbHandler handler = new DbHandler();
            handler.RegisterUser("Admin", "password");
            handler.LoginUser("Admin", "password");*/
        }
    }
}
