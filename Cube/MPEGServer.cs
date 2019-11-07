using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cube
{
    public class MPEGServer
    {
        private readonly UdpClient _server;

        public event EventHandler<byte[]> OnData;

        public MPEGServer()
        { 
            _server = new UdpClient(11000);
        }

        public void Run()
        {
            Console.WriteLine("UDP Server listenting...");
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 11000);
            while (true)
            {
                var res = _server.Receive(ref groupEP);
               // Console.WriteLine("Received data");
                OnData?.Invoke(this, res);
            }
        }
    }
}