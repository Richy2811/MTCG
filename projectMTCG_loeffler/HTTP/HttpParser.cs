using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace projectMTCG_loeffler {
    class HttpParser {

        private TcpClient _socket;
        private HttpRequestHandler _handler;
        private bool _mvpset;

        public string Method { get; private set; }
        public string Version { get; private set; }
        public string Path { get; private set; }

        public Dictionary<string, string> Headerparts { get; private set; }

        public HttpParser(TcpClient socket, HttpRequestHandler handler) {
            this._socket = socket;
            this._handler = handler;

            _mvpset = false;

            Headerparts = new Dictionary<string, string>();
        }

        private void Write(StreamWriter writer, string str) {
            //write to stream and output on console
            writer.WriteLine(str);
            Console.WriteLine(str);
        }

        public void Parse() {
            StreamWriter writer = new StreamWriter(_socket.GetStream());
            StreamReader reader = new StreamReader(_socket.GetStream());

            Console.WriteLine();

            string line;

            //read HTTP request information
            while((line = reader.ReadLine()) != null) {
                if(line.Length == 0) {
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

            //write a response
            string content = "<html><body><h1>Title</h1>Response</form></html>";

            Console.WriteLine();

            Write(writer, "HTTP/1.1 200 OK");
            Write(writer, "Server: My simple HttpServer");
            Write(writer, $"Current Time: {DateTime.Now}");
            Write(writer, $"Content-Length: {content.Length}");
            Write(writer, "Content-Type: text/html; charset=utf-8");
            Write(writer, "");
            Write(writer, content);

            writer.WriteLine();

            writer.Close();
        }
        
    }
}
