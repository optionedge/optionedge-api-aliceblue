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
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        void Warning(string message);
        
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
        /// Logs a warning message using Utils.LogMessage.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public void Warning(string message)
        {
            Utils.LogMessage($"WARNING: {message}");
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
        private readonly Random _random = new Random();
        private DateTime _lastReconnectAttempt = DateTime.MinValue;
        private readonly TimeSpan _minReconnectInterval = TimeSpan.FromSeconds(1);
        
        // Network interface tracking
        private string _lastKnownNetworkInterface = string.Empty;
        private DateTime _lastNetworkInterfaceCheck = DateTime.MinValue;
        private readonly TimeSpan _networkInterfaceCheckInterval = TimeSpan.FromSeconds(30);

        private System.Timers.Timer _connectionHealthCheck;
        private int _connectionHealthCheckInterval = 5000; // 5 seconds for faster detection
        private int _connectionTimeout = 5000; // 5 seconds timeout
        private DateTime _lastHeartbeatResponse = DateTime.MinValue;
        private DateTime _lastHeartbeatSent = DateTime.MinValue;
        private int _consecutiveHealthCheckFailures = 0;
        private int _maxHealthCheckFailures = 1; // Immediate reconnection after 1 failure
        private bool _networkWasDown = false;
        private DateTime _lastForceReconnectAttempt = DateTime.MinValue;
        private readonly TimeSpan _minForceReconnectInterval = TimeSpan.FromSeconds(2);
        
        // Rate limiting detection
        private int _consecutiveRateLimitErrors = 0;
        private readonly int _maxRateLimitErrors = 3;
        private DateTime _rateLimitBackoffUntil = DateTime.MinValue;
        
        // Half-open connection detection
        private DateTime _lastSuccessfulOperation = DateTime.MinValue;
        private readonly TimeSpan _halfOpenDetectionThreshold = TimeSpan.FromSeconds(30);
        
        // System load monitoring
        private DateTime _lastTimerExpectedTime = DateTime.MinValue;
        private readonly TimeSpan _maxTimerDelay = TimeSpan.FromSeconds(5);
        private bool _highSystemLoadDetected = false;
        
        // Dynamic reconnection strategy
        private bool _aggressiveReconnectMode = true;
        private int _reconnectCycleCount = 0;
        private readonly int _reconnectCycleThreshold = 5; // Switch modes every 5 cycles
        
        // Network change detection
        private System.Timers.Timer _networkCheckTimer;
        private int _networkCheckInterval = 2000; // Check network every 2 seconds
        private bool _lastNetworkState = true;
        
        // Locks for thread safety
        private readonly object _tickLock = new object();
        private readonly object _connectionLock = new object();
        private readonly object _reconnectLock = new object();
        private bool _reconnectionInProgress = false;

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
            // Initialize connection health check timer
            _connectionHealthCheck = new System.Timers.Timer();
            _connectionHealthCheck.Elapsed += ConnectionHealthCheckElapsed;
            _connectionHealthCheck.Interval = _connectionHealthCheckInterval;
            
            // Initialize network check timer
            _networkCheckTimer = new System.Timers.Timer();
            _networkCheckTimer.Elapsed += NetworkCheckTimerElapsed;
            _networkCheckTimer.Interval = _networkCheckInterval;
            _networkCheckTimer.Start(); // Start immediately to detect network changes
            
            // Register for application lifecycle events if available
            try
            {
                RegisterApplicationLifecycleEvents();
            }
            catch (Exception ex)
            {
                if (_debug)
                    _logger.Error("Failed to register application lifecycle events", ex);
            }
        }
        
        /// <summary>
        /// Checks if the network is available.
        /// </summary>
        /// <returns>True if the network is available, false otherwise.</returns>
        /// <summary>
        /// Registers for application lifecycle events if available.
        /// </summary>
        private void RegisterApplicationLifecycleEvents()
        {
            // This is a placeholder for platform-specific implementation
            // In a real implementation, you would register for events like:
            // - Application.Current.Suspending/Resuming (UWP)
            // - NSNotificationCenter.DefaultCenter.AddObserver (iOS)
            // - Activity.OnPause/OnResume (Android)
            
            // Since we can't implement platform-specific code here,
            // we'll just log that this would be implemented in a real app
            if (_debug)
                _logger.Debug("Application lifecycle events would be registered here in a platform-specific implementation");
        }
        
        /// <summary>
        /// Handles application suspend event.
        /// </summary>
        private void OnApplicationSuspend()
        {
            if (_debug)
                _logger.Debug("Application suspended, pausing timers");
            
            // Pause timers but don't close connection
            _timer?.Stop();
            _timerHeartbeat?.Stop();
            _connectionHealthCheck?.Stop();
            // Keep network check timer running to detect network changes when app resumes
        }
        
        /// <summary>
        /// Handles application resume event.
        /// </summary>
        private void OnApplicationResume()
        {
            if (_debug)
                _logger.Debug("Application resumed, checking connection state");
            
            // Check connection state and restart timers if needed
            if (IsConnected)
            {
                if (!_isReady)
                {
                    _logger.Warning("Connection exists but is not ready after resume. Forcing reconnection.");
                    ForceReconnect();
                }
                else
                {
                    // Restart timers
                    _timer?.Start();
                    _timerHeartbeat?.Start();
                    _connectionHealthCheck?.Start();
                    
                    // Send a heartbeat to verify connection is still alive
                    SendHeartBeat();
                }
            }
            else if (_isReconnect)
            {
                _logger.Warning("Not connected after resume. Forcing reconnection.");
                ForceReconnect();
            }
        }
        
        /// <summary>
        /// Detects if the network interface has changed.
        /// </summary>
        /// <returns>True if the network interface has changed, false otherwise.</returns>
        private bool HasNetworkInterfaceChanged()
        {
            // Only check periodically to avoid excessive overhead
            if (DateTime.Now - _lastNetworkInterfaceCheck < _networkInterfaceCheckInterval)
                return false;
                
            _lastNetworkInterfaceCheck = DateTime.Now;
            
            try
            {
                // Get the current active network interface
                string currentInterface = GetActiveNetworkInterface();
                
                // If this is the first check, just store the current interface
                if (string.IsNullOrEmpty(_lastKnownNetworkInterface))
                {
                    _lastKnownNetworkInterface = currentInterface;
                    return false;
                }
                
                // Check if the interface has changed
                if (_lastKnownNetworkInterface != currentInterface)
                {
                    if (_debug)
                        _logger.Debug($"Network interface changed from {_lastKnownNetworkInterface} to {currentInterface}");
                    
                    _lastKnownNetworkInterface = currentInterface;
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error("Error checking network interface change", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Gets the active network interface.
        /// </summary>
        /// <returns>A string identifying the active network interface.</returns>
        private string GetActiveNetworkInterface()
        {
            try
            {
                // This is a simplified implementation that just checks if any network is available
                // In a real implementation, you would use platform-specific APIs to get the actual interface
                // For example, NetworkInterface.GetAllNetworkInterfaces() on .NET
                
                if (IsNetworkAvailable())
                    return "ActiveNetwork"; // Placeholder for actual interface name
                else
                    return "NoNetwork";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        /// <summary>
        /// Detects if the connection is in a half-open state.
        /// </summary>
        /// <returns>True if the connection is in a half-open state, false otherwise.</returns>
        private bool IsConnectionHalfOpen()
        {
            // If we're connected but haven't had a successful operation in a while,
            // the connection might be half-open
            if (IsConnected &&
                _lastSuccessfulOperation != DateTime.MinValue &&
                DateTime.Now - _lastSuccessfulOperation > _halfOpenDetectionThreshold)
            {
                if (_debug)
                    _logger.Debug($"Possible half-open connection detected. Last successful operation was {(DateTime.Now - _lastSuccessfulOperation).TotalSeconds:0.0} seconds ago");
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if the system is under high load by monitoring timer delays.
        /// </summary>
        /// <param name="expectedTime">The time when the timer was expected to fire.</param>
        /// <returns>True if high system load is detected, false otherwise.</returns>
        private bool IsSystemUnderHighLoad(DateTime expectedTime)
        {
            if (expectedTime != DateTime.MinValue)
            {
                TimeSpan delay = DateTime.Now - expectedTime;
                
                if (delay > _maxTimerDelay)
                {
                    if (_debug)
                        _logger.Debug($"High system load detected. Timer delayed by {delay.TotalSeconds:0.0} seconds");
                    
                    _highSystemLoadDetected = true;
                    return true;
                }
            }
            
            // Reset the flag if the delay is acceptable
            _highSystemLoadDetected = false;
            return false;
        }
        
        /// <summary>
        /// Checks if the network is available.
        /// </summary>
        /// <returns>True if the network is available, false otherwise.</returns>
        private bool IsNetworkAvailable()
        {
            try
            {
                // Try to connect to a reliable DNS server (Google's public DNS)
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    // Set a short timeout
                    client.ReceiveTimeout = 1000;
                    client.SendTimeout = 1000;
                    
                    // Use BeginConnect which allows timeout without causing unobserved exceptions
                    var result = client.BeginConnect("8.8.8.8", 53, null, null);
                    
                    // Wait for the connection with a timeout
                    bool success = result.AsyncWaitHandle.WaitOne(1000, true);
                    
                    if (success && client.Connected)
                    {
                        // Properly close the connection
                        client.EndConnect(result);
                        return true;
                    }
                    else
                    {
                        // Close the socket if the connection attempt failed but the operation didn't time out
                        if (success)
                        {
                            try { client.EndConnect(result); } catch { }
                        }
                        
                        // Try Cloudflare DNS as backup
                        using (var backupClient = new System.Net.Sockets.TcpClient())
                        {
                            backupClient.ReceiveTimeout = 1000;
                            backupClient.SendTimeout = 1000;
                            
                            var backupResult = backupClient.BeginConnect("1.1.1.1", 53, null, null);
                            bool backupSuccess = backupResult.AsyncWaitHandle.WaitOne(1000, true);
                            
                            if (backupSuccess && backupClient.Connected)
                            {
                                backupClient.EndConnect(backupResult);
                                return true;
                            }
                            else
                            {
                                if (backupSuccess)
                                {
                                    try { backupClient.EndConnect(backupResult); } catch { }
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Handles network check timer elapsed event.
        /// </summary>
        private void NetworkCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Record the expected time for system load monitoring
                _lastTimerExpectedTime = DateTime.Now;
                
                // Check for high system load
                if (_highSystemLoadDetected)
                {
                    _logger.Warning("High system load detected during network check. Adjusting timers.");
                    // Adjust timers to reduce system load
                    AdjustTimersForHighLoad();
                }
                
                bool currentNetworkState = IsNetworkAvailable();
                
                // Check if network interface has changed
                if (HasNetworkInterfaceChanged())
                {
                    _logger.Warning("Network interface changed. Triggering reconnection.");
                    _networkWasDown = false;
                    _aggressiveReconnectMode = true;
                    _consecutiveHealthCheckFailures = _maxHealthCheckFailures;
                    ForceReconnect();
                }
                
                // If network state changed from down to up
                if (!_lastNetworkState && currentNetworkState)
                {
                    _logger.Warning("Network just became available. Triggering immediate reconnection.");
                    _networkWasDown = false;
                    _aggressiveReconnectMode = true; // Switch to aggressive mode
                    _consecutiveHealthCheckFailures = _maxHealthCheckFailures; // Force immediate reconnection
                    ForceReconnect();
                }
                // If network state changed from up to down
                else if (_lastNetworkState && !currentNetworkState)
                {
                    _logger.Warning("Network just became unavailable.");
                    _networkWasDown = true;
                }
                
                _lastNetworkState = currentNetworkState;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in network check timer", ex);
            }
        }
        
        /// <summary>
        /// Handles connection health check timer elapsed event.
        /// </summary>
        /// <summary>
        /// Adjusts timers to reduce system load.
        /// </summary>
        private void AdjustTimersForHighLoad()
        {
            try
            {
                // Increase intervals to reduce CPU usage
                if (_networkCheckTimer.Interval < 5000)
                    _networkCheckTimer.Interval = 5000;
                
                if (_connectionHealthCheck.Interval < 10000)
                    _connectionHealthCheck.Interval = 10000;
                
                if (_timerHeartbeat.Interval < 60000)
                    _timerHeartbeat.Interval = 60000;
            }
            catch (Exception ex)
            {
                _logger.Error("Error adjusting timers for high load", ex);
            }
        }
        
        /// <summary>
        /// Resets timer intervals to their default values.
        /// </summary>
        private void ResetTimerIntervals()
        {
            try
            {
                _networkCheckTimer.Interval = _networkCheckInterval;
                _connectionHealthCheck.Interval = _connectionHealthCheckInterval;
                _timerHeartbeat.Interval = _timerHeartbeatInterval;
            }
            catch (Exception ex)
            {
                _logger.Error("Error resetting timer intervals", ex);
            }
        }
        
        private void ConnectionHealthCheckElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Record the expected time for system load monitoring
                _lastTimerExpectedTime = DateTime.Now;
                
                // Check for high system load
                IsSystemUnderHighLoad(_lastTimerExpectedTime);
                
                // Always log the current connection state for debugging
                if (_debug)
                    _logger.Debug($"Connection health check - IsConnected: {IsConnected}, IsReady: {_isReady}, FailureCount: {_consecutiveHealthCheckFailures}, RateLimitErrors: {_consecutiveRateLimitErrors}");
                
                // Check if network is available
                bool networkAvailable = IsNetworkAvailable();
                
                if (!networkAvailable)
                {
                    _logger.Warning("Network appears to be down. Will attempt reconnection when network is available.");
                    _networkWasDown = true;
                    _consecutiveHealthCheckFailures++;
                    
                    // Even if network is down, try to reconnect after max failures
                    if (_consecutiveHealthCheckFailures >= _maxHealthCheckFailures)
                    {
                        _logger.Warning("Maximum failures reached even with network down. Attempting reconnection anyway.");
                        ForceReconnect();
                    }
                    return;
                }
                
                // If network was down but is now up, force reconnection
                if (_networkWasDown && networkAvailable)
                {
                    _logger.Warning("Network is back online. Forcing reconnection.");
                    _networkWasDown = false;
                    ForceReconnect();
                    return;
                }
                
                // Check for half-open connection
                if (IsConnectionHalfOpen())
                {
                    _logger.Warning("Half-open connection detected. Forcing reconnection.");
                    ForceReconnect();
                    return;
                }
                
                // Check if we're currently rate-limited
                if (_rateLimitBackoffUntil > DateTime.Now)
                {
                    if (_debug)
                        _logger.Debug($"Rate limit backoff in effect. Waiting until {_rateLimitBackoffUntil}");
                    return;
                }
                
                if (!IsConnected || !_isReady)
                {
                    _consecutiveHealthCheckFailures++;
                    _logger.Warning($"Connection health check: Not connected or not ready. Failure count: {_consecutiveHealthCheckFailures}");
                    
                    if (_isReconnect)
                    {
                        if (_consecutiveHealthCheckFailures >= _maxHealthCheckFailures)
                        {
                            _logger.Warning($"Maximum consecutive health check failures reached ({_maxHealthCheckFailures}). Forcing reconnection.");
                            ForceReconnect();
                        }
                        else
                        {
                            _logger.Warning("Attempting normal reconnect before reaching max failures.");
                            Reconnect();
                        }
                    }
                    else
                    {
                        _logger.Warning("Reconnect is disabled, but connection is not ready. Enabling reconnect.");
                        EnableReconnect();
                        Reconnect();
                    }
                    return;
                }
                
                // Reset failure counter if connection is healthy
                if (_consecutiveHealthCheckFailures > 0)
                {
                    if (_debug)
                        _logger.Debug($"Connection is healthy. Resetting failure count from {_consecutiveHealthCheckFailures} to 0.");
                    _consecutiveHealthCheckFailures = 0;
                }
                
                // Check if we've received a response since the last heartbeat
                if (_lastHeartbeatSent != DateTime.MinValue &&
                    _lastHeartbeatResponse < _lastHeartbeatSent &&
                    (DateTime.Now - _lastHeartbeatSent).TotalMilliseconds > _connectionTimeout)
                {
                    _logger.Warning($"Connection health check: No heartbeat response received in {_connectionTimeout}ms");
                    
                    // Force reconnection
                    ForceReconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in connection health check", ex);
                
                // Even if there's an error, increment failure count and try to reconnect
                _consecutiveHealthCheckFailures++;
                if (_consecutiveHealthCheckFailures >= _maxHealthCheckFailures)
                {
                    try
                    {
                        _logger.Warning("Error in health check but still attempting reconnection.");
                        ForceReconnect();
                    }
                    catch (Exception reconnectEx)
                    {
                        _logger.Error("Failed to force reconnect after health check error", reconnectEx);
                    }
                }
            }
        }
        
        /// <summary>
        /// Forces a reconnection regardless of the current state.
        /// </summary>
        /// <summary>
        /// Handles rate limiting by implementing exponential backoff.
        /// </summary>
        private void HandleRateLimiting()
        {
            _consecutiveRateLimitErrors++;
            
            if (_consecutiveRateLimitErrors >= _maxRateLimitErrors)
            {
                // Calculate backoff time with exponential increase and jitter
                double backoffSeconds = Math.Min(Math.Pow(2, _consecutiveRateLimitErrors) + _random.NextDouble(), 300); // Max 5 minutes
                _rateLimitBackoffUntil = DateTime.Now.AddSeconds(backoffSeconds);
                
                _logger.Warning($"Rate limiting detected. Backing off for {backoffSeconds:0.0} seconds until {_rateLimitBackoffUntil}");
            }
        }
        
        /// <summary>
        /// Resets rate limiting counters after successful operations.
        /// </summary>
        private void ResetRateLimiting()
        {
            _consecutiveRateLimitErrors = 0;
            _rateLimitBackoffUntil = DateTime.MinValue;
        }
        
        private void ForceReconnect()
        {
            // Use a lock to prevent concurrent reconnection attempts
            lock (_reconnectLock)
            {
                // Check if reconnection is already in progress
                if (_reconnectionInProgress)
                {
                    if (_debug)
                        _logger.Debug("Reconnection already in progress. Skipping this attempt.");
                    return;
                }
                
                _reconnectionInProgress = true;
                
                try
                {
                    // In aggressive mode, ignore the minimum interval
                    if (!_aggressiveReconnectMode && DateTime.Now - _lastForceReconnectAttempt < _minForceReconnectInterval)
                    {
                        _logger.Warning($"Force reconnect attempt too soon after previous attempt, delaying. Will try again in {(_minForceReconnectInterval - (DateTime.Now - _lastForceReconnectAttempt)).TotalSeconds:0.0} seconds.");
                        return;
                    }
                    
                    _lastForceReconnectAttempt = DateTime.Now;
                    
                    // Update reconnect cycle count and toggle mode if needed
                    _reconnectCycleCount++;
                    if (_reconnectCycleCount >= _reconnectCycleThreshold)
                    {
                        _aggressiveReconnectMode = !_aggressiveReconnectMode;
                        _reconnectCycleCount = 0;
                        _logger.Warning($"Switching reconnection mode to: {(_aggressiveReconnectMode ? "AGGRESSIVE" : "NORMAL")}");
                    }
                    
                    _logger.Warning($"FORCING RECONNECTION - {(_aggressiveReconnectMode ? "Aggressive" : "Normal")} reconnect strategy");
                    
                    // Make sure reconnect is enabled
                    if (!_isReconnect)
                    {
                        _logger.Warning("Reconnect was disabled. Enabling it now.");
                        EnableReconnect();
                    }
                    
                    // Close the current connection
                    try
                    {
                        if (IsConnected)
                        {
                            _logger.Warning("Closing existing connection before force reconnect");
                            _ws.Close(true);
                        }
                    }
                    catch (Exception closeEx)
                    {
                        _logger.Error("Error closing connection during force reconnect", closeEx);
                        // Continue anyway
                    }
                    
                    // Reset state
                    _isReady = false;
                    _retryCount = 0;
                    _lastReconnectAttempt = DateTime.MinValue; // Reset this to allow immediate reconnect
                    
                    // Stop and restart timers
                    try
                    {
                        _timer.Stop();
                        _timerHeartbeat.Stop();
                        _connectionHealthCheck.Stop();
                        
                        _timerTick = _aggressiveReconnectMode ? 1 : _interval; // Use very short interval in aggressive mode
                        _timer.Start();
                        _connectionHealthCheck.Start();
                        _networkCheckTimer.Start();
                    }
                    catch (Exception timerEx)
                    {
                        _logger.Error("Error managing timers during force reconnect", timerEx);
                        // Continue anyway
                    }
                    
                    // Create a new WebSocket instance if the current one is in a bad state
                    try
                    {
                        if (_ws == null || (_ws.IsConnected() && !_isReady))
                        {
                            _logger.Warning("Creating new WebSocket instance");
                            
                            // Unsubscribe from old events
                            if (_ws != null)
                            {
                                _ws.OnConnect -= HandleConnect;
                                _ws.OnData -= HandleData;
                                _ws.OnClose -= HandleClose;
                                _ws.OnError -= HandleError;
                            }
                            
                            // Create new instance
                            _ws = new WebSocket();
                            
                            // Subscribe to events
                            _ws.OnConnect += HandleConnect;
                            _ws.OnData += HandleData;
                            _ws.OnClose += HandleClose;
                            _ws.OnError += HandleError;
                        }
                    }
                    catch (Exception wsEx)
                    {
                        _logger.Error("Error recreating WebSocket during force reconnect", wsEx);
                        // Continue anyway
                    }
                    
                    // Directly call Connect to bypass the reconnection delay
                    _logger.Warning("Calling Connect() directly from ForceReconnect");
                    Connect();
                    
                    // Log the attempt
                    _logger.Warning("Force reconnect attempt completed");
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in ForceReconnect", ex);
                    
                    // Last resort - try to reconnect anyway
                    try
                    {
                        _ws = new WebSocket();
                        _ws.OnConnect += HandleConnect;
                        _ws.OnData += HandleData;
                        _ws.OnClose += HandleClose;
                        _ws.OnError += HandleError;
                        
                        _ws.Connect(_socketUrl);
                    }
                    catch (Exception lastEx)
                    {
                        _logger.Error("Last resort reconnection also failed", lastEx);
                    }
                }
                finally
                {
                    // Always reset the flag when done
                    _reconnectionInProgress = false;
                }
            }
        }

        internal void SetShouldUnSubscribeHandler(Func<int,bool> shouldUnSubscribe )
        {
            _shouldUnSubscribe = shouldUnSubscribe; 
        }

        private void TimerHeartbeatElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Record the expected time for system load monitoring
            _lastTimerExpectedTime = DateTime.Now;
            
            // Check for high system load
            IsSystemUnderHighLoad(_lastTimerExpectedTime);
            
            if (IsConnected)
            {
                SendHeartBeat();
                _lastHeartbeatSent = DateTime.Now;
            }
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
                
                // Increment failure count
                _consecutiveHealthCheckFailures++;
            }
        }

        private void HandleError(string Message)
        {
            try
            {
                _isReady = false;
                _logger.Error($"WebSocket error: {Message}");
                _timerTick = _interval;
                _timer.Start();
                OnError?.Invoke(Message);
                
                if (_isReconnect && !IsConnected)
                {
                    Reconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in HandleError handler", ex);
            }
        }

        private void HandleClose()
        {
            try
            {
                _isReady = false;
                _timer.Stop();
                _timerHeartbeat.Stop();
                _connectionHealthCheck.Stop();
                // Keep network check timer running to detect network changes
                OnClose?.Invoke();
                
                if (_isReconnect)
                {
                    _timerTick = _interval;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in HandleClose handler", ex);
            }
        }

        /// <summary>
        /// Closes the WebSocket connection and stops all timers.
        /// </summary>
        public void Close()
        {
            try
            {
                _isReady = false;
                _cts?.Cancel();
                _subscribedTokens?.Clear();
                _ws?.Close();
                _timer?.Stop();
                _timerHeartbeat?.Stop();
                _connectionHealthCheck?.Stop();
                // Keep network check timer running to detect network changes
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
                
                // Update heartbeat response time for any message received
                _lastHeartbeatResponse = DateTime.Now;
                
                // Update last successful operation time
                _lastSuccessfulOperation = DateTime.Now;
                
                // Reset failure counter on successful message
                _consecutiveHealthCheckFailures = 0;
                
                // Reset rate limiting counters
                ResetRateLimiting();

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
                            lock (_tickLock)
                            {
                                if (!_isReady)
                                {
                                    _isReady = true;
                                    
                                    // Reset connection health metrics on successful connection
                                    _consecutiveHealthCheckFailures = 0;
                                    _lastHeartbeatResponse = DateTime.Now;
                                    
                                    OnReady?.Invoke();
                                }
                            }

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
                    _logger.Warning("Received CLOSE message from server");
                    _isReady = false;
                    
                    if (_isReconnect)
                    {
                        _timerTick = _interval;
                        _timer.Start();
                    }
                    else
                    {
                        Close();
                    }
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
            _connectionHealthCheck.Start();
            _networkCheckTimer.Start();

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
            // Use a lock to prevent concurrent reconnection attempts
            lock (_reconnectLock)
            {
                // Check if reconnection is already in progress
                if (_reconnectionInProgress)
                {
                    if (_debug)
                        _logger.Debug("Reconnection already in progress. Skipping this attempt.");
                    return;
                }
                
                _reconnectionInProgress = true;
                
                try
                {
                    lock (_connectionLock)
                    {
                        // Check if network is available before attempting reconnection
                        if (!IsNetworkAvailable())
                        {
                            _logger.Warning("Network appears to be down. Will attempt reconnection when network is available.");
                            _networkWasDown = true;
                            return;
                        }
                        
                        // In aggressive mode, ignore the minimum interval
                        if (!_aggressiveReconnectMode && DateTime.Now - _lastReconnectAttempt < _minReconnectInterval)
                        {
                            if (_debug)
                                _logger.Debug("Reconnect attempt too soon after previous attempt, delaying");
                            return;
                        }
                        
                        _lastReconnectAttempt = DateTime.Now;
                        
                        if (_debug)
                            _logger.Debug($"Attempting to reconnect");
                        if (IsConnected && _isReady)
                        {
                            if (_debug)
                                _logger.Debug($"Already connected and ready, no need to reconnect");
                            return;
                        }

                        if (_retryCount > _retries)
                        {
                            _logger.Warning($"Maximum retry count reached: {_retryCount} > {_retries}");
                            _ws.Close(true);
                            
                            // Reset retry count instead of disabling reconnect
                            // This allows the system to try again after a break
                            _retryCount = 0;
                            
                            // Only disable reconnect if explicitly requested
                            // DisableReconnect();
                            OnNoReconnect?.Invoke();
                        }
                        else
                        {
                            OnReconnect?.Invoke();
                            _retryCount += 1;
                            
                            // Close the connection if it's in a bad state
                            if (IsConnected && !_isReady)
                            {
                                _logger.Warning("Connection exists but is not ready. Closing before reconnect.");
                                _ws.Close(true);
                            }
                            
                            // Add jitter to prevent thundering herd problem
                            int jitter = _random.Next(0, 1000);
                            Thread.Sleep(jitter);
                            
                            // Attempt to connect
                            if (_debug)
                                _logger.Debug($"Reconnect attempt #{_retryCount} of {_retries}");
                            Connect();
                            
                            // Dynamic reconnection strategy
                            if (_aggressiveReconnectMode)
                            {
                                // In aggressive mode, use very short intervals
                                _timerTick = Math.Min(_retryCount, 3); // 1, 2, 3 seconds for first attempts
                            }
                            else
                            {
                                // Use shorter backoff for initial retries, then exponential
                                if (_retryCount <= 3)
                                {
                                    _timerTick = _interval;
                                }
                                else
                                {
                                    // Exponential backoff with max of 60 seconds
                                    _timerTick = (int)Math.Min(Math.Pow(1.5, _retryCount) * _interval, 60);
                                }
                            }
                            
                            if (_debug)
                                _logger.Debug($"Next reconnect attempt in {_timerTick} seconds if needed");
                            _timer.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in Reconnect", ex);
                    
                    // Still try to reconnect despite the error
                    _timerTick = _interval;
                    _timer.Start();
                }
                finally
                {
                    // Always reset the flag when done
                    _reconnectionInProgress = false;
                }
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
            try
            {
                if (_debug) _logger.Debug("Resubscribing to market data");
                
                if (!IsConnected || !_isReady)
                {
                    _logger.Warning("Cannot resubscribe: not connected or not ready");
                    return;
                }

                SubscriptionToken[] allTokens = _subscribedTokens.Keys.ToArray();
                
                if (allTokens.Length == 0)
                {
                    if (_debug) _logger.Debug("No tokens to resubscribe");
                    return;
                }

                SubscriptionToken[] quoteTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_QUOTE).ToArray();
                SubscriptionToken[] fullTokens = allTokens.Where(key => _subscribedTokens[key] == Constants.TICK_MODE_FULL).ToArray();

                // Add small delay between unsubscribe and subscribe to avoid overwhelming the server
                if (quoteTokens.Length > 0)
                {
                    UnSubscribe(quoteTokens);
                    Thread.Sleep(100);
                    Subscribe(Constants.TICK_MODE_QUOTE, quoteTokens);
                }
                
                if (fullTokens.Length > 0)
                {
                    UnSubscribe(fullTokens);
                    Thread.Sleep(100);
                    Subscribe(Constants.TICK_MODE_FULL, fullTokens);
                }
                
                if (_debug) _logger.Debug($"Resubscribed to {quoteTokens.Length} quote tokens and {fullTokens.Length} full tokens");
                
                // Update last successful operation time
                _lastSuccessfulOperation = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in ReSubscribe", ex);
            }
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
                    _connectionHealthCheck?.Dispose();
                    _networkCheckTimer?.Dispose();
                    
                    if (_ws != null)
                    {
                        _ws.OnConnect -= HandleConnect;
                        _ws.OnData -= HandleData;
                        _ws.OnClose -= HandleClose;
                        _ws.OnError -= HandleError;
                        
                        // Assuming IWebSocket implements IDisposable
                        (_ws as IDisposable)?.Dispose();
                    }
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
