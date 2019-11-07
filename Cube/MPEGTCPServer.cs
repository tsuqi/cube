using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cube
{
    public class MPEGTCPServer
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _localEP;

        public event EventHandler<byte[]> OnData;

        public MPEGTCPServer()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            _localEP = new IPEndPoint(IPAddress.Any, 11000);

            _socket = new Socket(_localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public Task Run()
        {
            _socket.Bind(_localEP);
            _socket.Listen(10);

            Console.WriteLine("TCP Server listenting...");
            _socket.BeginAccept(new AsyncCallback(AcceptSocket), _socket);

            return Task.CompletedTask;
        }

        private void AcceptSocket(IAsyncResult ar)
        {
            MPEGState state = new MPEGState();
            state.Socket = _socket.EndAccept(ar);

            Console.WriteLine("Incoming TCP socket accepted!");
            state.Socket.BeginReceive(state.Buffer, 0, MPEGState.BufferSize, 0,
                new AsyncCallback(OnIncomingData), state);

            _socket.BeginAccept(new AsyncCallback(AcceptSocket), _socket);
        }

        private void OnIncomingData(IAsyncResult ar)
        {
            MPEGState state = (MPEGState)ar.AsyncState;
            Socket handler = state.Socket;

            int read = handler.EndReceive(ar);
            if (read > 0)
            {
                OnData?.Invoke(this, state.Buffer);
                handler.BeginReceive(state.Buffer, 0, MPEGState.BufferSize, 0,
                    new AsyncCallback(OnIncomingData), state);
            }
            else
            {
                handler.Close();
            }
        }
    }

    public class MPEGState
    {
        public Socket Socket = null;
        public const int BufferSize = 1200 * 1024;
        public byte[] Buffer = new byte[BufferSize];
    }
}