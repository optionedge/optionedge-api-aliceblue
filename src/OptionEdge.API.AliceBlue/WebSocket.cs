using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Net;

namespace OptionEdge.API.AliceBlue
{
    public delegate void OnSocketConnectHandler();
    public delegate void OnSocketCloseHandler();
    public delegate void OnSocketErrorHandler(string Message);
    public delegate void OnSocketDataHandler(byte[] Data, int Count, string MessageType);

    internal class WebSocket : IWebSocket
    {
        ClientWebSocket _ws;
        string _url;
        int _bufferLength;

        public event OnSocketConnectHandler OnConnect;
        public event OnSocketCloseHandler OnClose;
        public event OnSocketDataHandler OnData;
        public event OnSocketErrorHandler OnError;

        CancellationTokenSource _cts = null;

        public WebSocket(int BufferLength = 2000000)
        {            
            _bufferLength = BufferLength;
        }

        public bool IsConnected()
        {
            if(_ws is null)
                return false;
            
            return _ws.State == WebSocketState.Open;
        }

        public async Task ConnectAsync(string url, Dictionary<string, string> headers = null)
        {
            _url = url;
            _cts = new CancellationTokenSource();
            byte[] buffer = new byte[_bufferLength];

            try
            {
                _ws = new ClientWebSocket();

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        _ws.Options.SetRequestHeader(header.Key, header.Value);
                    }
                }

                await _ws.ConnectAsync(new Uri(_url), _cts.Token);
                OnConnect?.Invoke();

                // Start receiving in background
                _ = Task.Run(() => ReceiveLoopAsync(buffer, _cts.Token));
            }
            catch (WebSocketException wsEx)
            {
                OnError?.Invoke($"WebSocket error during connect: {wsEx.Message}");
                OnClose?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Unexpected error during connect: {ex.Message}");
                OnClose?.Invoke();
            }
        }

        private async Task ReceiveLoopAsync(byte[] buffer, CancellationToken token)
        {
            try
            {
                while (_ws != null && _ws.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        OnClose?.Invoke();
                        break;
                    }

                    int offset = result.Count;
                    bool endOfMessage = result.EndOfMessage;

                    // Handle fragmented messages
                    while (!endOfMessage)
                    {
                        var tempResult = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer, offset, buffer.Length - offset), token);
                        offset += tempResult.Count;
                        endOfMessage = tempResult.EndOfMessage;
                    }

                    try
                    {
                        OnData?.Invoke(buffer, offset, result.MessageType.ToString());
                    }
                    catch (Exception handlerEx)
                    {
                        OnError?.Invoke($"Error in socket data processing: {handlerEx}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected if token was cancelled during shutdown
            }
            catch (WebSocketException wsEx)
            {
                OnError?.Invoke($"WebSocket receive error: {wsEx.Message}");
                OnClose?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Receive loop error: {ex.Message}");
                OnClose?.Invoke();
            }
        }


        public void Send(string Message)
        {
            if (_ws.State == WebSocketState.Open)
                try
                {
                    _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(Message)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                }
                catch (Exception e)
                {
                    OnError?.Invoke("Error while sending data. Message:  " + e.Message);
                }
        }

        public void Close(bool Abort = false)
        {
            if(_ws.State == WebSocketState.Open)
            {
                try
                {
                    if (Abort)
                        _ws.Abort();
                    else
                    {
                        _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
                        OnClose?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke("Error while closing connection. Message: " + e.Message);
                }
            }
        }
    }
}
