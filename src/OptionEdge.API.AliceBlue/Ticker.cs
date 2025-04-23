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
    /// <summary>
    /// Interface for logging messages from the Ticker class.
    /// </summary>
    public interface ITickerLogger
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Debug(string message);
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">Optional exception that caused the error.</param>
        void Error(string message, Exception exception = null);
    }
    
    /// <summary>
    /// Default implementation of ITickerLogger that logs to the Utils.LogMessage method.
    /// </summary>
    public class DefaultTickerLogger : ITickerLogger
    {
        /// <summary>
        /// Logs a debug message using Utils.LogMessage.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Debug(string message)
        {
            Utils.LogMessage(message);
        }
        
        /// <summary>
        /// Logs an error message using Utils.LogMessage.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">Optional exception that caused the error.</param>
        public void Error(string message, Exception exception = null)
        {
            if (exception != null)
                Utils.LogMessage($"{message}: {exception}");
            else
                Utils.LogMessage(message);
        }
    }
    
    /// <summary>
    /// Class for connecting to AliceBlue's market data feed via WebSocket.
    /// </summary>
    public class Ticker : IDisposable
    {
        private bool _disposed = false;
        
        private readonly bool _debug = false;
        private readonly ITickerLogger _logger;

        private string _userId;
        private string _accessToken;
        
        private string _socketUrl = Constants.DEFAULT_WEBSOCKET_URL;
        private bool _isReconnect = false;
        private int _interval = 5;
        private int _retries = 50;
        private int _retryCount = 0;

        private System.Timers.Timer _timer;
        private int _timerTick = 5;

        private IWebSocket _ws;

        private bool _isReady;

        /// <summary>
        /// Token -> Mode Mapping
        /// </summary>
        private ConcurrentDictionary<SubscriptionToken, string> _subscribedTokens;

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

        private Func<int, bool> _shouldUnSubscribe = null;

        private System.Timers.Timer _timerHeartbeat;
        private int _timerHeartbeatInterval = 40000;

        /// <summary>
        /// Initializes a new instance of the Ticker class for connecting to AliceBlue's market data feed.
        /// </summary>
        /// <param name="userId">The user ID for authentication.</param>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <param name="socketUrl">Optional WebSocket URL. If null, the default URL will be used.</param>
        /// <param name="reconnect">Whether to automatically reconnect on disconnection.</param>
        /// <param name="reconnectInterval">The interval in seconds between reconnection attempts.</param>
        /// <param name="reconnectTries">The maximum number of reconnection attempts.</param>
        /// <param name="debug">Whether to enable debug logging.</param>
        /// <summary>
        /// Cancellation token source for cancelling operations.
        /// </summary>
        private CancellationTokenSource _cts;
        
        /// <summary>
        /// Initializes a new instance of the Ticker class for connecting to AliceBlue's market data feed.
        /// </summary>
        /// <param name="userId">The user ID for authentication.</param>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <param name="socketUrl">Optional WebSocket URL. If null, the default URL will be used.</param>
        /// <param name="reconnect">Whether to automatically reconnect on disconnection.</param>
        /// <param name="reconnectInterval">The interval in seconds between reconnection attempts.</param>
        /// <param name="reconnectTries">The maximum number of reconnection attempts.</param>
        /// <param name="debug">Whether to enable debug logging.</param>
        /// <param name="logger">Optional custom logger. If null, a default logger will be used.</param>
        public Ticker(string userId, string accessToken, string socketUrl = null, bool reconnect = false, int reconnectInterval = 5, int reconnectTries = 50, bool debug = false, ITickerLogger logger = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "Access token cannot be null or empty");
                
            _debug = debug;
            _userId = userId;
            _accessToken = accessToken;
            _logger = logger ?? new DefaultTickerLogger();
            _subscribedTokens = new ConcurrentDictionary<SubscriptionToken, string>();
            _interval = reconnectInterval;
            _timerTick = reconnectInterval;
            _retries = reconnectTries;
            _isReconnect = reconnect;

            _socketUrl = string.IsNullOrEmpty(socketUrl) ? Constants.DEFAULT_WEBSOCKET_URL : socketUrl;

            _ws = new WebSocket();

            _ws.OnConnect += HandleConnect;
            _ws.OnData += HandleData;
            _ws.OnClose += HandleClose;
            _ws.OnError += HandleError;

            _timer = new System.Timers.Timer();
            _timer.Elapsed += OnTimerTick;
            _timer.Interval = 1000;

            _timerHeartbeat = new System.Timers.Timer();
            _timerHeartbeat.Elapsed += TimerHeartbeatElapsed;
            _timerHeartbeat.Interval = _timerHeartbeatInterval;
        }

        internal void SetShouldUnSubscribeHandler(Func<int,bool> shouldUnSubscribe )
        {
            _shouldUnSubscribe = shouldUnSubscribe; 
        }

        private void TimerHeartbeatElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsConnected)
                SendHeartBeat();
        }

        private void SendHeartBeat()
        {
            try
            {
                if (!_ws.IsConnected()) return;
                string msg = Constants.HEARTBEAT_MESSAGE;
                _ws.Send(msg);
            }
            catch (Exception ex)
            {
                // Use the OnError event instead of Console.WriteLine for better error handling
                OnError?.Invoke($"Error sending heartbeat: {ex.Message}");
                
                // Still log the full exception details when debug is enabled
                if (_debug)
                    _logger.Error("AliceBlue Market Ticker:Send Heartbeat error", ex);
            }
        }

        private void HandleError(string Message)
        {
            _timerTick = _interval;
            _timer.Start();
            OnError?.Invoke(Message);
        }

        private void HandleClose()
        {
            _timer.Stop();
            _timerHeartbeat.Stop();
            OnClose?.Invoke();
        }

        /// <summary>
        /// Closes the WebSocket connection and stops all timers.
        /// </summary>
        public void Close()
        {
            try
            {
                _cts?.Cancel();
                _subscribedTokens?.Clear();
                _ws?.Close();
                _timer?.Stop();
                _timerHeartbeat?.Stop();
            }
            catch (Exception ex)
            {
                if (_debug)
                    _logger.Error("Error closing ticker", ex);
            }
        }

        private void HandleData(byte[] Data, int Count, string MessageType)
        {
            try
            {
                _timerTick = _interval;

                if (MessageType == Constants.WEBSOCKET_MESSAGE_TYPE_TEXT)
                {
                    try
                    {
                        var tick = JsonSerializer.Deserialize<Tick>(Data.Take(Count).ToArray(), 0);
                        if (tick == null)
                        {
                            if (_debug)
                                _logger.Debug("Failed to deserialize tick data: null result");
                            return;
                        }

                        if (tick.ResponseType == Constants.SOCKET_RESPONSE_TYPE_CONNECTION_ACKNOWLEDGEMENT)
                        {
                            _isReady = true;

                            OnReady?.Invoke();

                            if (_subscribedTokens.Count > 0)
                                ReSubscribe();

                            if (_debug)
                                _logger.Debug("Connection acknowledgement received. Websocket connected.");
                        }
                        else if (tick.ResponseType == Constants.SOCKET_RESPONSE_TYPE_TICK_ACKNOWLEDGEMENT ||
                                 tick.ResponseType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH_ACKNOWLEDGEMENT)
                        {
                            OnTick?.Invoke(tick);
                        }
                        else if (tick.ResponseType == Constants.SOCKET_RESPONSE_TYPE_TICK ||
                                 tick.ResponseType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH)
                        {
                            OnTick?.Invoke(tick);
                        }
                        else
                        {
                            if (_debug)
                                _logger.Debug($"Unknown feed type: {tick.ResponseType}");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Error processing tick data: {ex.Message}");
                        if (_debug)
                            _logger.Error("Error deserializing or processing tick data", ex);
                    }
                }
                else if (MessageType == Constants.WEBSOCKET_MESSAGE_TYPE_CLOSE)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error in data handler: {ex.Message}");
                if (_debug)
                    _logger.Error("Unhandled exception in HandleData", ex);
            }
        }

        private void OnTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerTick--;
            if (_timerTick < 0)
            {
                _timer.Stop();
                if (_isReconnect)
                    Reconnect();
            }
            if (_debug) _logger.Debug($"Timer tick: {_timerTick}");
        }

        private void HandleConnect()
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
            _timerHeartbeat.Start();

            OnConnect?.Invoke();
        }

        /// <summary>
        /// Gets a value indicating whether the WebSocket connection is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _ws.IsConnected(); }
        }

        /// <summary>
        /// Gets a value indicating whether the WebSocket connection is ready to receive market data.
        /// </summary>
        public bool IsReady
        {
            get { return _isReady; }
        }

        /// <summary>
        /// Connects to the AliceBlue WebSocket server.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the connection.</param>
        public void Connect(CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a new cancellation token source linked to the provided token
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                _timerTick = _interval;
                _timer.Start();
                if (!IsConnected)
                {
                    // Connect to the WebSocket
                    _ws.Connect(_socketUrl);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error connecting to WebSocket: {ex.Message}");
                if (_debug)
                    _logger.Error("Connect error", ex);
                
                // Start the reconnection timer if reconnection is enabled
                if (_isReconnect)
                {
                    _timerTick = _interval;
                    _timer.Start();
                }
            }
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
                // Use exponential backoff for reconnection attempts with a maximum of 60 seconds
                if (_debug) _logger.Debug($"New reconnection interval: {_timerTick} seconds");
                _timer.Start();
            }
        }

        /// <summary>
        /// Subscribes to market data for the specified tokens in the given mode.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., NSE, BSE).</param>
        /// <param name="mode">The subscription mode (QUOTE or FULL).</param>
        /// <param name="tokens">Array of security tokens to subscribe to.</param>
        public void Subscribe(string exchange, string mode, int[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return;
            
            var subscriptionTokens = ConvertToSubscriptionTokens(exchange, tokens);
            Subscribe(mode, subscriptionTokens);
        }

        /// <summary>
        /// Subscribes to market data for the specified subscription tokens in the given mode.
        /// </summary>
        /// <param name="mode">The subscription mode (QUOTE or FULL).</param>
        /// <param name="tokens">Array of subscription tokens to subscribe to.</param>
        public void Subscribe(string mode, SubscriptionToken[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return;

            var subscriptionRequst = new SubscribeFeedDataRequest
            {
                SubscriptionTokens = tokens,
                RequestType = mode == Constants.TICK_MODE_QUOTE ? Constants.SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_MARKET : Constants.SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_DEPTH,
            };

            var requestJson = JsonSerializer.ToJsonString(subscriptionRequst);

            if (_debug) _logger.Debug($"Subscribe request JSON length: {requestJson.Length}");

            if (IsConnected)
                _ws.Send(requestJson);

            foreach (SubscriptionToken token in subscriptionRequst.SubscriptionTokens)
            {
                _subscribedTokens.AddOrUpdate(token, mode, (key, oldValue) => mode);
            }
        }

        /// <summary>
        /// Unsubscribes from market data for the specified tokens.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., NSE, BSE).</param>
        /// <param name="tokens">Array of security tokens to unsubscribe from.</param>
        public void UnSubscribe(string exchange, int[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return;
            
            var subscriptionTokens = ConvertToSubscriptionTokens(exchange, tokens);
            UnSubscribe(subscriptionTokens);
        }

        /// <summary>
        /// Unsubscribes from market data for the specified subscription tokens.
        /// </summary>
        /// <param name="tokens">Array of subscription tokens to unsubscribe from.</param>
        public void UnSubscribe(SubscriptionToken[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return;

            var request = new UnsubscribeMarketDataRequest
            {
                SubscribedTokens = tokens.Where(x => _shouldUnSubscribe != null ? _shouldUnSubscribe.Invoke(x.Token) : true).ToArray(),
            };

            var requestJson = JsonSerializer.ToJsonString(request);

            if (_debug) _logger.Debug($"Unsubscribe request JSON length: {requestJson.Length}");

            if (IsConnected)
                _ws.Send(requestJson);

            foreach (SubscriptionToken token in request.SubscribedTokens)
            {
                _subscribedTokens.TryRemove(token, out _);
            }
        }

        private void ReSubscribe()
        {
            if (_debug) _logger.Debug("Resubscribing to market data");

            SubscriptionToken[] allTokens = _subscribedTokens.Keys.ToArray();

            SubscriptionToken[] quoteTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_QUOTE).ToArray();
            SubscriptionToken[] fullTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_FULL).ToArray();

            UnSubscribe(quoteTokens);
            UnSubscribe(fullTokens);

            Subscribe(Constants.TICK_MODE_QUOTE, quoteTokens);
            Subscribe(Constants.TICK_MODE_FULL, fullTokens);
        }

        /// <summary>
        /// Enables automatic reconnection on disconnection.
        /// </summary>
        /// <param name="interval">The interval in seconds between reconnection attempts.</param>
        /// <param name="retries">The maximum number of reconnection attempts.</param>
        public void EnableReconnect(int interval = 5, int retries = 50)
        {
            _isReconnect = true;
            _interval = Math.Max(interval, 5);
            _retries = retries;

            _timerTick = _interval;
            if (IsConnected)
                _timer.Start();
        }

        /// <summary>
        /// Disables automatic reconnection on disconnection.
        /// </summary>
        public void DisableReconnect()
        {
            _isReconnect = false;
            if (IsConnected)
                _timer.Stop();
            _timerTick = _interval;
        }
        
        /// <summary>
        /// Disposes all resources used by the Ticker instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases the unmanaged resources used by the Ticker and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    Close();
                    _timer?.Dispose();
                    _timerHeartbeat?.Dispose();
                    // Assuming IWebSocket implements IDisposable
                    (_ws as IDisposable)?.Dispose();
                }
                
                // Free unmanaged resources
                
                _disposed = true;
            }
        }
        
        /// <summary>
        /// Finalizer to ensure resources are cleaned up if Dispose is not called.
        /// </summary>
        ~Ticker()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Converts an array of token integers to an array of SubscriptionToken objects.
        /// </summary>
        /// <param name="exchange">The exchange code.</param>
        /// <param name="tokens">Array of token integers.</param>
        /// <returns>Array of SubscriptionToken objects.</returns>
        private SubscriptionToken[] ConvertToSubscriptionTokens(string exchange, int[] tokens)
        {
            return tokens.Select(token => new SubscriptionToken
            {
                Token = token,
                Exchange = exchange
            }).ToArray();
        }
    }
}
