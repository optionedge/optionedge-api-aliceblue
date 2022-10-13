using OptionEdge.API.AliceBlue.Records;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Utf8Json;

namespace OptionEdge.API.AliceBlue
{
    public class Ticker
    {
        private bool _debug = false;

        private string _userId;
        private string _accessToken;
        
        private string _socketUrl = "wss://ws1.aliceblueonline.com/NorenWS";
        private bool _isReconnect = false;
        private int _interval = 5;
        private int _retries = 50;
        private int _retryCount = 0;

        System.Timers.Timer _timer;
        int _timerTick = 5;

        private IWebSocket _ws;

        bool _isReady;

        /// <summary>
        /// Token -> Mode Mapping
        /// </summary>
        private Dictionary<SubscriptionToken, string> _subscribedTokens;

        public delegate void OnConnectHandler();
        public delegate void OnReadyHandler();
        public delegate void OnCloseHandler();
        public delegate void OnTickHandler(Tick TickData);
        public delegate void OnErrorHandler(string Message);
        public delegate void OnReconnectHandler();
        public delegate void OnNoReconnectHandler();
        
        public event OnConnectHandler OnConnect;
        public event OnReadyHandler OnReady;
        public event OnCloseHandler OnClose;
        public event OnTickHandler OnTick;
        public event OnErrorHandler OnError;
        public event OnReconnectHandler OnReconnect;
        public event OnNoReconnectHandler OnNoReconnect;

        Func<int, bool> _shouldUnSubscribe = null;

        public Ticker(string userId, string accessToken, string socketUrl = null, bool reconnect = false, int reconnectInterval = 5, int reconnectTries = 50, bool debug = false)
        {
            _debug = debug;
            _userId = userId;
            _accessToken = accessToken;
            _subscribedTokens = new Dictionary<SubscriptionToken, string>();
            _interval = reconnectInterval;
            _timerTick = reconnectInterval;
            _retries = reconnectTries;
            _isReconnect = reconnect;

            if (string.IsNullOrEmpty(socketUrl))
                _socketUrl = "wss://ws1.aliceblueonline.com/NorenWS";
            else
                _socketUrl = socketUrl;

            _ws = new WebSocket();

            _ws.OnConnect += _onConnect;
            _ws.OnData += _onData;
            _ws.OnClose += _onClose;
            _ws.OnError += _onError;

            _timer = new System.Timers.Timer();
            _timer.Elapsed += _onTimerTick;
            _timer.Interval = 1000;
        }

        internal void SetShouldUnSubscribeHandler(Func<int,bool> shouldUnSubscribe )
        {
            _shouldUnSubscribe = shouldUnSubscribe; 
        }

        private void _onError(string Message)
        {
            _tickStore?.Clear();
            _timerTick = _interval;
            _timer.Start();
            OnError?.Invoke(Message);
        }

        private void _onClose()
        {
            _tickStore?.Clear();
            _timer.Stop();
            OnClose?.Invoke();
        }
       
        ConcurrentDictionary<string, ConcurrentDictionary<int, Tick>> _tickStore = new ConcurrentDictionary<string, ConcurrentDictionary<int, Tick>>();

        private void FormatTick(ref Tick tick)
        {
            if (_tickStore.ContainsKey(tick.Exchange) && _tickStore[tick.Exchange].ContainsKey(tick.Token.Value))
            {
                ConcurrentDictionary<int, Tick> exchangeStore = null;
                Tick storedTick = null;

                _tickStore.TryGetValue(tick.Exchange, out exchangeStore);
                exchangeStore.TryGetValue(tick.Token.Value, out storedTick);

                tick.PreviousDayClose = storedTick.PreviousDayClose;
                tick.ChangeValue = storedTick.ChangeValue;

                if (tick.BuyPrice1 <= 0)
                    tick.BuyPrice1 = storedTick.BuyPrice1;
                else
                    storedTick.BuyPrice1 = tick.BuyPrice1;

                if (tick.BuyPrice2 <= 0)
                    tick.BuyPrice2 = storedTick.BuyPrice2;
                else
                    storedTick.BuyPrice2 = tick.BuyPrice2;

                if (tick.BuyPrice3 <= 0)
                    tick.BuyPrice3 = storedTick.BuyPrice3;
                else
                    storedTick.BuyPrice3 = tick.BuyPrice3;

                if (tick.BuyPrice4 <= 0)
                    tick.BuyPrice4 = storedTick.BuyPrice4;
                else
                    storedTick.BuyPrice4 = tick.BuyPrice4;

                if (tick.BuyPrice5 <= 0)
                    tick.BuyPrice5 = storedTick.BuyPrice5;
                else
                    storedTick.BuyPrice5 = tick.BuyPrice5;


                if (tick.SellPrice1 <= 0)
                    tick.SellPrice1 = storedTick.SellPrice1;
                else
                    storedTick.SellPrice1 = tick.SellPrice1;

                if (tick.SellPrice2 <= 0)
                    tick.SellPrice2 = storedTick.SellPrice2;
                else
                    storedTick.SellPrice2 = tick.SellPrice2;

                if (tick.SellPrice3 <= 0)
                    tick.SellPrice3 = storedTick.SellPrice3;
                else
                    storedTick.SellPrice3 = tick.SellPrice3;
                
                if (tick.SellPrice4 <= 0)
                    tick.SellPrice4 = storedTick.SellPrice4;
                else
                    storedTick.SellPrice4 = tick.SellPrice4;
                
                if (tick.SellPrice5 <= 0)
                    tick.SellPrice5 = storedTick.SellPrice5;
                else
                    storedTick.SellPrice5 = tick.SellPrice5;


                if (tick.BuyQty1 <= 0)
                    tick.BuyQty1 = storedTick.BuyQty1;
                else
                    storedTick.BuyQty1 = tick.BuyQty1;

                if (tick.BuyQty2 <= 0)
                    tick.BuyQty2 = storedTick.BuyQty2;
                if (tick.BuyQty3 <= 0)
                    tick.BuyQty3 = storedTick.BuyQty3;
                if (tick.BuyQty4 <= 0)
                    tick.BuyQty4 = storedTick.BuyQty4;
                if (tick.BuyQty5 <= 0)
                    tick.BuyQty5 = storedTick.BuyQty5;

                if (tick.SellQty1 <= 0)
                    tick.SellQty1 = storedTick.SellQty1;
                else
                    storedTick.SellQty1 = tick.SellQty1;

                if (tick.SellQty2 <= 0)
                    tick.SellQty2 = storedTick.SellQty2;
                if (tick.SellQty3 <= 0)
                    tick.SellQty3 = storedTick.SellQty3;
                if (tick.SellQty4 <= 0)
                    tick.SellQty4 = storedTick.SellQty4;
                if (tick.SellQty5 <= 0)
                    tick.SellQty5 = storedTick.SellQty5;


            }
        }

        private void AddToTickStore(Tick tick)
        {
            if (!_tickStore.ContainsKey(tick.Exchange))
                _tickStore.TryAdd(tick.Exchange, new ConcurrentDictionary<int, Tick>());

            if (!_tickStore[tick.Exchange].ContainsKey(tick.Token.Value))
            {
                decimal close;
                decimal changeValue;
                if (tick.Close.HasValue && tick.Close.Value > 0)
                {
                    close = tick.Close.Value;
                    changeValue = tick.LastTradedPrice.Value - tick.Close.Value;
                }
                else
                {
                    changeValue = tick.LastTradedPrice.Value * (tick.PercentageChange.Value / 100);
                    if (Math.Sign(tick.PercentageChange.Value) == 1)
                        close = tick.LastTradedPrice.Value - changeValue;
                    else
                        close = tick.LastTradedPrice.Value + changeValue;
                }

                tick.PreviousDayClose = close;
                tick.ChangeValue = changeValue;

                _tickStore[tick.Exchange].TryAdd(tick.Token.Value, tick);
            }

            FormatTick(ref tick);
        }

        private void _onData(byte[] Data, int Count, string MessageType)
        {
            if (_debug) Utils.LogMessage("On Data event");

            _timerTick = _interval;

            if (MessageType == "Text")
            {
                string message = Encoding.UTF8.GetString(Data.Take(Count).ToArray());
                if (_debug) Utils.LogMessage("WebSocket Message: " + message);

                var data = JsonSerializer.Deserialize<dynamic>(Data);
                if (data["t"] == "ck")
                {
                    _isReady = true;

                    OnReady();

                    if (_subscribedTokens.Count > 0)
                        ReSubscribe();

                    if (_debug)
                        Utils.LogMessage("Connection acknowledgement received. Websocket connected.");
                }
                else if (data["t"] == "tk" || data["t"] == "dk")
                {
                    Tick tick = new Tick(data);
                    AddToTickStore(tick);
                    OnTick(tick);
                } else if (data["t"] == "tf" || data["t"] == "df")
                {
                    Tick tick = new Tick(data);
                    FormatTick(ref tick);
                    OnTick(tick);
                }
                else
                {
                    if (_debug)
                        Utils.LogMessage($"Unknown feed type: {data["t"]}");
                }
            }
            else if (MessageType == "Close")
            {
                Close();
            }
        }

        private void _onTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerTick--;
            if (_timerTick < 0)
            {
                _timer.Stop();
                if (_isReconnect)
                    Reconnect();
            }
            if (_debug) Utils.LogMessage(_timerTick.ToString());
        }

        private void _onConnect()
        {
            _ws.Send(JsonSerializer.ToJsonString(new CreateWebsocketConnectionRequest
            {
                AccessToken = _accessToken,
                AccountId = _userId + "_API",
                UserId = _userId + "_API"
            }));

            _retryCount = 0;
            _timerTick = _interval;
            _timer.Start();

            OnConnect?.Invoke();
        }

        public bool IsConnected
        {
            get { return _ws.IsConnected(); }
        }

        public bool IsReady
        {
            get { return _isReady; }
        }

        public void Connect()
        {
            _timerTick = _interval;
            _timer.Start();
            if (!IsConnected)
            {
                _ws.Connect(_socketUrl);
            }
        }

        public void Close()
        {
            _tickStore?.Clear();
            _timer.Stop();
            _ws.Close();
        }

        private void Reconnect()
        {
            if (IsConnected)
                return;

            if (_retryCount > _retries)
            {
                _ws.Close(true);
                DisableReconnect();
                OnNoReconnect?.Invoke();
            }
            else
            {
                OnReconnect?.Invoke();
                _retryCount += 1;
                _ws.Close(true);
                Connect();
                _timerTick = (int)Math.Min(Math.Pow(2, _retryCount) * _interval, 60);
                if (_debug) Utils.LogMessage("New interval " + _timerTick);
                _timer.Start();
            }
        }

        public void Subscribe(string exchnage, string mode, int[] tokens)
        {
            var subscriptionTokens = tokens.Select(token => new SubscriptionToken
            {
                Token = token,
                Exchange = exchnage
            }).ToArray();

            Subscribe(mode, subscriptionTokens);
        }

        public void Subscribe(string mode, SubscriptionToken[] tokens)
        {
            if (tokens.Length == 0) return;

            var subscriptionRequst = new SubscribeFeedDataRequest
            {
                SubscriptionTokens = tokens,
                RequestType = mode == Constants.TICK_MODE_QUOTE ? Constants.SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_MARKET : Constants.SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_DEPTH,
            };

            var requestJson = JsonSerializer.ToJsonString(subscriptionRequst);

            if (_debug) Utils.LogMessage(requestJson.Length.ToString());

            if (IsConnected)
                _ws.Send(requestJson);

            foreach (SubscriptionToken token in subscriptionRequst.SubscriptionTokens)
            {
                if (_subscribedTokens.ContainsKey(token))
                    _subscribedTokens[token] = mode; 
                else
                    _subscribedTokens.Add(token, mode);
            }
        }

        public void UnSubscribe(string exchnage, int[] tokens)
        {
            var subscriptionTokens = tokens.Select(token => new SubscriptionToken
            {
                Token = token,
                Exchange = exchnage
            }).ToArray();

            UnSubscribe(subscriptionTokens);
        }

        public void UnSubscribe(SubscriptionToken[] tokens)
        {
            if (tokens.Length == 0) return;

            var request = new UnsubscribeMarketDataRequest
            {
                SubscribedTokens = tokens.Where(x => _shouldUnSubscribe != null ? _shouldUnSubscribe.Invoke(x.Token) : true).ToArray(),
            };

            var requestJson = JsonSerializer.ToJsonString(request);

            if (_debug) Utils.LogMessage(requestJson.Length.ToString());

            if (IsConnected)
                _ws.Send(requestJson);

            foreach (SubscriptionToken token in request.SubscribedTokens)
                if (_subscribedTokens.ContainsKey(token))
                    _subscribedTokens.Remove(token);
        }

        private void ReSubscribe()
        {
            if (_debug) Utils.LogMessage("Resubscribing");

            SubscriptionToken[] allTokens = _subscribedTokens.Keys.ToArray();

            SubscriptionToken[] quoteTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_QUOTE).ToArray();
            SubscriptionToken[] fullTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_FULL).ToArray();

            UnSubscribe(quoteTokens);
            UnSubscribe(fullTokens);

            Subscribe(Constants.TICK_MODE_QUOTE, quoteTokens);
            Subscribe(Constants.TICK_MODE_FULL, fullTokens);
        }

        public void EnableReconnect(int interval = 5, int retries = 50)
        {
            _isReconnect = true;
            _interval = Math.Max(interval, 5);
            _retries = retries;

            _timerTick = _interval;
            if (IsConnected)
                _timer.Start();
        }

        public void DisableReconnect()
        {
            _isReconnect = false;
            if (IsConnected)
                _timer.Stop();
            _timerTick = _interval;
        }
    }
}
