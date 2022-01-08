using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace projectMTCG_loeffler {
    class HttpRequestHandler {
        private int _port;
        private TcpListener _listener;

        public HttpRequestHandler(int port) {
            this._port = port;
        }

        public void Run() {
            //create listener and start
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start(5);

            Console.WriteLine("Listener has started");

            while(true) {
                //accept incoming requests in loop
                TcpClient socket = _listener.AcceptTcpClient();
                socket.ReceiveTimeout = 50;

                Console.WriteLine("Incoming request accepted");

                HttpParser parser = new HttpParser(socket, this);

                Thread th = new Thread(parser.Parse);
                th.Start();
            }
        }
    }
}
