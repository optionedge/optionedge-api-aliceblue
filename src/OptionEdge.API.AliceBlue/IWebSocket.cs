using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionEdge.API.AliceBlue
{
    public interface IWebSocket
    {
        event OnSocketConnectHandler OnConnect;
        event OnSocketCloseHandler OnClose;
        event OnSocketDataHandler OnData;
        event OnSocketErrorHandler OnError;
        bool IsConnected();
        Task ConnectAsync(string Url, Dictionary<string, string> headers = null);
        void Send(string Message);
        void Close(bool Abort = false);
    }
}
