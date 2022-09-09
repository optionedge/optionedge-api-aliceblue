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

        public void Connect(string Url, Dictionary<string, string> headers = null)
        {
            _url = Url;
            try
            {
                _ws = new ClientWebSocket();
                if (headers != null)
                {
                    foreach (string key in headers.Keys)
                    {
                        _ws.Options.SetRequestHeader(key, headers[key]);
                    }
                }
                _ws.ConnectAsync(new Uri(_url), CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                var aggregateErrorMessage = e.InnerExceptions.Select(x => x.Message).Aggregate((x,y) => x + Environment.NewLine + y);

                OnError?.Invoke("Error while connecting websocket. Message: " + e.ToString() + Environment.NewLine + aggregateErrorMessage);
                if (e.ToString().Contains("Forbidden") && e.ToString().Contains("403"))
                {
                    OnClose?.Invoke();
                }
                return;
            }
            catch (Exception e)
            {
                OnError?.Invoke("Error while connecting websocket. Message:  " + e.Message);
                return;
            }
            OnConnect?.Invoke();

            byte[] buffer = new byte[_bufferLength];
            Action<Task<WebSocketReceiveResult>> callback = null;

            try
            {
               callback = t =>
                {
                    try
                    {
                        byte[] tempBuff = new byte[_bufferLength];
                        int offset = t.Result.Count;
                        bool endOfMessage = t.Result.EndOfMessage;

                        while (!endOfMessage)
                        {
                            WebSocketReceiveResult result = _ws.ReceiveAsync(new ArraySegment<byte>(tempBuff), CancellationToken.None).Result;
                            Array.Copy(tempBuff, 0, buffer, offset, result.Count);
                            offset += result.Count;
                            endOfMessage = result.EndOfMessage;
                        }

                        try
                        {
                            OnData?.Invoke(buffer, offset, t.Result.MessageType.ToString());
                        }catch (Exception e)
                        {
                            OnError?.Invoke($"Error in socket data processing handler.{e.ToString()}");
                        }

                        _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(callback);
                    }catch(Exception e)
                    {
                        if(IsConnected())
                            OnError?.Invoke("Error while recieving data. Message:  " + e.Message);
                        else
                            OnError?.Invoke("Lost ticker connection.");
                    }
                };

                _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(callback);
            }
            catch (Exception e)
            {
                OnError?.Invoke("Error while recieving data. Message:  " + e.Message);
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
