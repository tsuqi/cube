using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cube.Controllers
{
    [ApiController]
    public class VideoStreamController : ControllerBase
    {
        //reminder everything in this project needs to go into src/Cube
        // this controller is responsible for handling clients that are looking at the video stream
        private readonly ILogger _logger;
        private readonly static HashSet<WebSocket> _sockets = new HashSet<WebSocket>();

        public VideoStreamController(ILogger<VideoStreamController> logger)
        {
            _logger = logger;
        }

        static VideoStreamController()
        {
            Program.MPEGServer.OnData += (object sender, byte[] data) =>
            {
                Task.Run(async () =>
                {
                    foreach(var ws in _sockets)
                    {
                        await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, new CancellationTokenSource().Token);
                    }
                });
            };
        }

        [Route("/api/_internal/stream")]
        public async Task StreamAsync()
        {
            if(!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            // authentication code.

            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _sockets.Add(ws);

            _logger.LogInformation("Client connected.");

            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                while (ws.State == WebSocketState.Open)
                {
                    byte[] buffer = new byte[1024];
                    var response = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (response.MessageType == WebSocketMessageType.Close)
                        break;
                }

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed due to client closing", cts.Token);
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                    default:
                        _sockets.Remove(ws);
                        return;
                }
            }

            _sockets.Remove(ws);
            return;
        }
    }
}
