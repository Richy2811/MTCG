﻿using System;

namespace projectMTCG_loeffler {
    class Program {
        static void Main(string[] args) {
            HttpRequestHandler handler = new HttpRequestHandler(2811);
            handler.Run();
        }
    }
}
