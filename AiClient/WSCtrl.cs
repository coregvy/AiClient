using AiClient.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiClient
{
    internal class WSCtrl
    {
        private ClientWebSocket socket = null;
        public event EventHandler ReceiveMessage;
        private CancellationTokenSource onMsgCancel;
        private string ServerUrl = "www.tomiko.cf/red";
        private string Protocol = "wss";
        public WSCtrl()
        {
            ServerUrl = Settings.Default.ServerUrl;
            Protocol = Settings.Default.IsSecure ? "wss" : "ws";
        }

        public async Task OpenAsync(RoomInfo room, string userName, WSMessage fstMsg)
        {
            socket = new ClientWebSocket();
            var url = new Uri($"{Protocol}://{ServerUrl}/api/con4/{room.Path}");
            Debug.WriteLine($"connection ws: {url.AbsolutePath}");
            await socket.ConnectAsync(url, CancellationToken.None);
            Debug.WriteLine($"send login: {fstMsg}");
            _ = SendMessage(fstMsg);
            onMsgCancel = new CancellationTokenSource();
            while (!onMsgCancel.IsCancellationRequested)
            {
                await OnMessage(onMsgCancel.Token);
            }
        }

        public async Task SendMessage(WSMessage message)
        {
            var msgStr = message.ToString();
            Debug.WriteLine($"ws send: {msgStr}");
            var buffer = Encoding.UTF8.GetBytes(msgStr);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task OnMessage(CancellationToken cancelToken)
        {
            var buffer = new byte[1024];
            Debug.WriteLine($"on msg while begin. {socket.State}");
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    Debug.WriteLine("ws receive wait");
                    var segment = new ArraySegment<byte>(buffer);
                    var result = await socket.ReceiveAsync(segment, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK",
                          CancellationToken.None);
                        break;
                    }
                    int count = result.Count;
                    while (!result.EndOfMessage)
                    {
                        if (count >= buffer.Length)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                              "That's too long", CancellationToken.None);
                            break;
                        }
                        segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                        result = await socket.ReceiveAsync(segment, CancellationToken.None);

                        count += result.Count;
                    }

                    //メッセージを取得
                    //var message = Encoding.UTF8.GetString(buffer, 0, count);
                    ReceiveMessage?.Invoke(JSON.Parse<WSMessage>(buffer, count), null);
                    //Debug.WriteLine($"ws msg: {message}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"WS Failure: {e.Message}, state: {socket.State}");
                if (socket.State != WebSocketState.Open)
                {
                    onMsgCancel.Cancel();
                }
            }
        }

        public async Task CloseAsync()
        {
            Debug.WriteLine("ws close begin.");
            onMsgCancel.Cancel();
            await socket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            socket.Dispose();
            socket = null;
        }
    }

    [DataContract]
    public class WSMessage
    {
        [DataMember(Name = "call")]
        public string Call { get; set; }
        // call = login
        [DataMember(Name = "user0")]
        public string User0 { get; set; }
        [DataMember(Name = "user1")]
        public string User1 { get; set; }
        [DataMember(Name = "ready")]
        public bool Ready { get; set; }
        // call = step
        [DataMember(Name = "player")]
        public int Player { get; set; }
        [DataMember(Name = "stdout")]
        public string Stdout { get; set; }
        [DataMember(Name = "stdin")]
        public string Stdin { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        public override string ToString()
        {
            return JSON.ToString(this);
        }
    }
}