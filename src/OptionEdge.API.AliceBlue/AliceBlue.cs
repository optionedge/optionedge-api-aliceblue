using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using OptionEdge.API.AliceBlue.Records;
using RestSharp;

namespace OptionEdge.API.AliceBlue
{
    public class AliceBlue : IDisposable
    {
        string _baseUrl = "https://ant.aliceblueonline.com/rest/AliceBlueAPIService/api/";
        string _websocketUrl = "wss://ws1.aliceblueonline.com/NorenWS";

        string _apiKey;
        string _userId;
        string _accessToken;

        bool _enableLogging;

        protected readonly RestClient _restClient;

        readonly Dictionary<string, string> _urls = new Dictionary<string, string>
        {
            ["auth.encryption.key"] = "/customer/getAPIEncpkey",
            ["auth.session.id"] = "/customer/getUserSID",

            ["portfolio.position.book"] = "/positionAndHoldings/positionBook",
            ["portfolio.holdings"] = "/positionAndHoldings/holdings",

            ["order.place"] = "/placeOrder/executePlaceOrder",

            ["order.book"] = "/placeOrder/fetchOrderBook",
            ["trade.book"] = "/placeOrder/fetchTradeBook",
            ["order.history"] = "/placeOrder/orderHistory",

            ["order.modify"] = "/placeOrder/modifyOrder",
            ["order.cancel"] = "/placeOrder/cancelOrder",
            ["order.bracket.exit"] = "/placeOrder/exitBracketOrder",
            ["order.cover.exit"] = "/placeOrder/exitCoverOrder",

            ["funds.limits"] = "/limits/getRmsLimits",

            ["profile.account"] = "/customer/accountDetails",

            ["contract.master.csv"] = "https://v2api.aliceblueonline.com/restpy/static/contract_master/{EXCHANGE}.csv",

            ["square.off.position"] = "/positionAndHoldings/sqrOofPosition",
            
            ["history"] = "https://a3-chart.aliceblueonline.com/omk/rest/ChartAPIService/chart/history",
            //["history"] = "/chart/history",

            ["scrip.quote"] = "/ScripDetails/getScripQuoteDetails",
            ["scrip.open.interest"] = "/marketWatch/scripsMW",

            ["ws.session.create"] = "/ws/createSocketSess",
            ["ws.session.invalidate"] = "/ws/invalidateSocketSess",

            ["basket.margin"] = "/basket/getMargin",
        };

        public AliceBlue()
        {

        }

        public AliceBlue(string userId, string apiKey, string baseUrl = null, string websocketUrl = null, bool enableLogging = false, Action<string> onAccessTokenGenerated = null, Func<string> cachedAccessTokenProvider = null)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException("User id required.");
            if (string.IsNullOrEmpty(apiKey)) throw new ArgumentNullException("Api key required.");

            _apiKey = apiKey;
            _userId = userId;

            if (!string.IsNullOrEmpty(baseUrl)) _baseUrl = baseUrl;
            if (!string.IsNullOrEmpty(websocketUrl)) _websocketUrl = websocketUrl;

            _enableLogging = enableLogging;

            var options = new RestClientOptions(_baseUrl);

            options.Authenticator = new AliceBlueAuthenticator(_userId,
                _apiKey,
                _baseUrl,
                _urls["auth.encryption.key"],
                _urls["auth.session.id"],
                enableLogging, (accessToken) =>
                {
                    onAccessTokenGenerated?.Invoke(accessToken);
                },
                cachedAccessTokenProvider,
                (accessToken) =>
                {
                    _accessToken = accessToken;
                });

            _restClient = new RestClient(options);
            
            //_restClient.UseNewtonsoftJson();

        }

        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        public async Task<EncryptionKeyResult> GetEncryptionKey()
        {
            if (_enableLogging)
                Utils.LogMessage("Getting the Encryption Key...");

            var options = new RestClientOptions(_baseUrl);
            var restClient = new RestClient(options);

            var request = new RestRequest(_urls["auth.encryption.key"]);

            var encryptionKeyParams = new EncryptionKeyParams
            {
                UserId = _userId,
            };

            request.AddStringBody(JsonConvert.SerializeObject(encryptionKeyParams), ContentType.Json);

            if (_enableLogging)
                Utils.LogMessage($"Calling encryption key endpoint: {_urls["auth.encryption.key"]}");

            var encryptionKeyResponse = await restClient.PostAsync<EncryptionKeyResult>(request);

            if (_enableLogging)
                Utils.LogMessage($"Encryption Key Result. Status: {encryptionKeyResponse.Status}-{encryptionKeyResponse.ErrorMessage}");

            return encryptionKeyResponse;
        }

        public async Task<GetAccessTokenResult> GetAccessToken(string encryptionKey)
        {
            if (_enableLogging)
                Utils.LogMessage("Getting the Access Token (Session Id).");

            
            var userData = Utils.GetSHA256($"{_userId}{_apiKey}{encryptionKey}");

            var options = new RestClientOptions(_baseUrl);
            var restClient = new RestClient(options);

            var request = new RestRequest(_urls["auth.session.id"]);

            var createSessionIdParams = new WebsocketSessionIdParams
            {
                UserId = _userId,
                UserData = userData
            };
            request.AddStringBody(JsonConvert.SerializeObject(createSessionIdParams), ContentType.Json);

            if (_enableLogging)
                Utils.LogMessage($"Calling create session endpoint: {_urls["auth.session.id"]}");

            var accessTokenResult = await restClient.PostAsync<GetAccessTokenResult>(request);
            

            if (restClient != null) restClient.Dispose();

            return accessTokenResult;
        }

        private Ticker _ticker;
        public virtual async Task<Ticker> CreateTicker()
        {
            // Only single ticker instance allowed
            if (_ticker != null) return _ticker;

            await CreateWebsocketSessionAsync();

            _ticker = new Ticker(_userId, GetWebsocketAccessToken(), socketUrl: _websocketUrl, debug: _enableLogging);

            return _ticker;
        }

        protected void SetShouldUnSubscribeHandler(Func<int, bool> shouldUnSubscribe)
        {
            _ticker.SetShouldUnSubscribeHandler(shouldUnSubscribe);
        }


        private async Task CreateWebsocketSessionAsync()
        {
            try
            {
                var response = await ExecutePostAsync<WebsocketSessionResult>(
                    _urls["ws.session.invalidate"],
                    new WebsocketAccessTokenParams
                    {
                        LoginType = "API"
                    });

                response = await ExecutePostAsync<WebsocketSessionResult>(
                    _urls["ws.session.create"],
                    new WebsocketAccessTokenParams
                    {
                        LoginType = "API"
                    });
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    Utils.LogMessage($"Error creating websocket session: {ex.Message}");
            }
        }
        
        // Legacy method for backward compatibility
        private void CreateWebsocketSession()
        {
            CreateWebsocketSessionAsync().GetAwaiter().GetResult();
        }

        private string GetWebsocketAccessToken()
        {
            return Utils.GetSHA256(Utils.GetSHA256(_accessToken));
        }

        public virtual async Task<FundsResult[]> GetFunds()
        {
            return await ExecuteGetAsync<FundsResult[]>(_urls["funds.limits"]);
        }

        /// <summary>
        /// Square off an existing position
        /// </summary>
        /// <param name="squareOffPositionParams">Parameters for square off position</param>
        /// <returns>Result of the square off operation</returns>
        public virtual async Task<SquareOffPositionResult> SquareOffPosition(SquareOffPositionParams squareOffPositionParams)
        {
            if (squareOffPositionParams == null)
                throw new ArgumentNullException(nameof(squareOffPositionParams), "Square off position parameters cannot be null");
                
            return await ExecutePostAsync<SquareOffPositionResult>(_urls["square.off.position"], squareOffPositionParams);
        }

        public virtual async Task<OrderHistoryResult[]> GetOrderHistory(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                throw new ArgumentNullException(nameof(orderNumber), "Order number cannot be null or empty");
                
            return await ExecutePostAsync<OrderHistoryResult[]>(_urls["order.history"], new OrderHistoryParams { OrderNumber = orderNumber });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryDelay">milliseconds</param>
        /// <returns></returns>
        public virtual async Task<OrderHistoryResult> GetOrderHistory(string orderNumber, string orderStatus, int maxRetries = 5, int retryDelay = 500)
        {
            return await GetOrderHistory(orderNumber, (status) =>
            {
                return status == orderStatus;
            }, maxRetries, retryDelay);         
        }

        public virtual async Task<OrderHistoryResult> GetOrderHistory(string orderNumber, Func<string, bool> hasOrderStatus, int maxRetries = 5, int retryDelay = 500)
        {
            if (string.IsNullOrEmpty(orderNumber))
                throw new ArgumentNullException(nameof(orderNumber), "Order number cannot be null or empty");
                
            if (hasOrderStatus == null)
                throw new ArgumentNullException(nameof(hasOrderStatus), "Order status predicate cannot be null");
                
            int retry = 1;
            OrderHistoryResult orderHistory = null;

            while (retry <= maxRetries)
            {
                retry++;

                var orderHistories = await GetOrderHistory(orderNumber);
                if (orderHistories != null && orderHistories.Length > 0)
                {
                    orderHistory = orderHistories.Where(x => hasOrderStatus(x.OrderStatus)).FirstOrDefault();
                }

                if (orderHistory == null)
                {
                    await Task.Delay(retryDelay);
                    continue;
                }
                else
                    break;
            }

            return orderHistory;
        }

        public virtual async Task<OrderBookResult[]> GetOrderBook()
        {
            return await ExecuteGetAsync<OrderBookResult[]>(_urls["order.book"]);
        }

        public virtual async Task<TradeBookResult[]> GetTradeBook()
        {
            return await ExecuteGetAsync<TradeBookResult[]>(_urls["trade.book"]);
        }

        public virtual async Task<HistoryDataResult> GetHistoricalData(HistoryDataParams historyDataParams)
        {
            if (historyDataParams == null)
                throw new ArgumentNullException(nameof(historyDataParams), "History data parameters cannot be null");
                
            var historicalDataBaseUrl = "https://a3-chart.aliceblueonline.com/omk/rest/ChartAPIService/chart/history";

            HistoryDataResult result = null;

            using (var restClient = new RestClient(historicalDataBaseUrl))
            {
                var bearer = $"Bearer {_userId} {_accessToken}";
                var headers = new HeaderParameter(KnownHeaders.Authorization, bearer);
                restClient.DefaultParameters.AddParameter(headers);

                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddQueryParameter("exchange", historyDataParams.Exchange);
                request.AddQueryParameter("symbol", historyDataParams.InstrumentToken);
                request.AddQueryParameter("from", historyDataParams.From);
                request.AddQueryParameter("to", historyDataParams.To);
                request.AddQueryParameter("resolution", historyDataParams.Interval);
                request.AddQueryParameter("user", _userId);

                var response = await restClient.ExecuteGetAsync<HistoryDataResult>(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Data != null)
                {
                    result = response.Data;

                    for (int i = 0; i < response.Data.Close.Length; i++)
                    {
                        result.Candles.Add(new HistoryCandle
                        {
                            Open = response.Data.Open[i],
                            Close = response.Data.Close[i],
                            High = response.Data.High[i],
                            Low = response.Data.Low[i],
                            Volume = response.Data.Volume[i],
                            IV = response.Data.IV,
                            TimeData = response.Data.Time[i]
                        });
                    }
                }
            }

            return result;
        }

        public virtual async Task<HistoryDataResult> GetHistoricalData(string exchange, int instrumentToken, DateTime from, DateTime to, string interval, bool index = false)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException(nameof(exchange), "Exchange cannot be null or empty");
                
            if (string.IsNullOrEmpty(interval))
                throw new ArgumentNullException(nameof(interval), "Interval cannot be null or empty");
                
            HistoryDataParams historyDataParams = new HistoryDataParams
            {
                Exchange = exchange,
                InstrumentToken = instrumentToken,
                From = ((DateTimeOffset)from).ToUnixTimeSeconds(),
                To = ((DateTimeOffset)to).ToUnixTimeSeconds(),
                Interval = interval,
            };

            return await GetHistoricalData(historyDataParams);
        }

        public virtual async Task<ModifyOrderResult> ModifyOrder(ModifyOrderParams modifyOrderParams)
        {
            if (modifyOrderParams == null)
                throw new ArgumentNullException(nameof(modifyOrderParams), "Modify order parameters cannot be null");
                
            return await ExecutePostAsync<ModifyOrderResult>(_urls["order.modify"], modifyOrderParams);
        }

        public virtual async Task<CancelOrderResult> CancelOrder(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                throw new ArgumentNullException(nameof(orderNumber), "Order number cannot be null or empty");
                
            return await ExecutePostAsync<CancelOrderResult>(_urls["order.cancel"], new CancelOrderParams
            {
                OrderNumber = orderNumber
            });
        }

        public virtual async Task<ExitBracketOrderResult> ExitBracketOrder(ExitBracketOrderParams exitBracketOrderParams)
        {
            if (exitBracketOrderParams == null)
                throw new ArgumentNullException(nameof(exitBracketOrderParams), "Exit bracket order parameters cannot be null");
                
            return await ExecutePostAsync<ExitBracketOrderResult>(_urls["order.bracket.exit"], exitBracketOrderParams);
        }

        public virtual async Task<ExitCoverOrderResult> ExitCoverOrder(ExitCoverOrderParams exitCoverOrder)
        {
            if (exitCoverOrder == null)
                throw new ArgumentNullException(nameof(exitCoverOrder), "Exit cover order parameters cannot be null");
                
            return await ExecutePostAsync<ExitCoverOrderResult>(_urls["order.cover.exit"], exitCoverOrder);
        }

        public virtual async Task<OpenInterestResult[]> GetOpenInterest(string exchange, int[] tokens)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException(nameof(exchange), "Exchange cannot be null or empty");
                
            if (tokens == null || tokens.Length == 0)
                throw new ArgumentException("Tokens array cannot be null or empty", nameof(tokens));
                
            var openInterestParams = new OpenInterestParams
            {
                OpenInterestTokens = tokens.Select(token => new OpenInterestToken
                {
                    Exchange = exchange,
                    InstrumentToken = token
                }).ToArray()
            };
       
            return await GetOpenInterest(openInterestParams);
        }

        public virtual async Task<OpenInterestResult[]> GetOpenInterest(OpenInterestParams tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens), "Open interest parameters cannot be null");
                
            if (tokens.OpenInterestTokens == null || tokens.OpenInterestTokens.Length == 0)
                throw new ArgumentException("Open interest tokens cannot be null or empty", nameof(tokens));
                
            var openInterestParamsInternal = new OpenInterestParamsInternal
            {
                OpenInterestTokens = tokens.OpenInterestTokens,
                UserId = _userId,
            };

            return await ExecutePostAsync<OpenInterestResult[]>(_urls["scrip.open.interest"], openInterestParamsInternal);
        }

        public virtual async Task<AccountDetails> GetAccountDetails()
        {
            return await ExecuteGetAsync<AccountDetails>(_urls["profile.account"]);
        }

        public virtual async Task<PositionBookResult[]> GetPositionBookDayWise()
        {
            return await GetPosition(Constants.POSITION_DAYWISE);
        }
        
        public virtual async Task<PositionBookResult[]> GetPositionBookNetWise()
        {
            return await GetPosition(Constants.POSITION_NETWISE);
        }

        protected virtual async Task<PositionBookResult[]> GetPosition(string retentionType)
        {
            if (string.IsNullOrEmpty(retentionType))
                throw new ArgumentNullException(nameof(retentionType), "Retention type cannot be null or empty");
                
            return await ExecutePostAsync<PositionBookResult[]>(
                _urls["portfolio.position.book"],
                new PositionBookParams
                {
                    RetentionType = retentionType
                });
        }
        public virtual async Task<ScriptQuoteResult> GetScripQuote(string exchange, int instrumentToken)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException(nameof(exchange), "Exchange cannot be null or empty");
                
            return await ExecutePostAsync<ScriptQuoteResult>(
                _urls["scrip.quote"],
                new ScriptQuoteParams
                {
                    Exchange = exchange.ToUpper(),
                    InstrumentToken = instrumentToken
                });
        }

        public virtual async Task<HoldingsResult> GetHoldings()
        {
            return await ExecuteGetAsync<HoldingsResult>(_urls["portfolio.holdings"]);
        }

        public virtual async Task<PlaceRegularOrderResult> PlaceOrder(PlaceRegularOrderParams order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order parameters cannot be null");
                
            var placeOrderResult = await PlaceOrder(new PlaceRegularOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public virtual async Task<PlaceCoverOrderResult> PlaceOrder(PlaceCoverOrderParams order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order parameters cannot be null");
                
            var placeOrderResult = await PlaceOrder(new PlaceCoverOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public virtual async Task<PlaceBracketOrderResult> PlaceOrder(PlaceBracketOrderParams order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order parameters cannot be null");
                
            var placeOrderResult = await PlaceOrder(new PlaceBracketOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public virtual async Task<PlaceCoverOrderResult[]> PlaceOrder(PlaceCoverOrderParams[] orders)
        {
            if (orders == null || orders.Length == 0)
                throw new ArgumentException("Orders array cannot be null or empty", nameof(orders));
                
            return await PlaceCoverOrder(orders);
        }

        public virtual async Task<PlaceBracketOrderResult[]> PlaceOrder(PlaceBracketOrderParams[] orders)
        {
            if (orders == null || orders.Length == 0)
                throw new ArgumentException("Orders array cannot be null or empty", nameof(orders));
                
            return await PlaceBracketOrder(orders);
        }

        public virtual async Task<PlaceRegularOrderResult[]> PlaceOrder(PlaceRegularOrderParams[] orders)
        {
            if (orders == null || orders.Length == 0)
                throw new ArgumentException("Orders array cannot be null or empty", nameof(orders));
                
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                if (string.IsNullOrEmpty(order.ProductCode))
                    throw new ArgumentNullException(nameof(order.ProductCode), "Product code required.");

                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_REGULAR;
            }

            return await ExecutePostAsync<PlaceRegularOrderResult[]>(_urls["order.place"], orders);
        }

        public virtual async Task<PlaceCoverOrderResult> PlaceCoverOrder(PlaceCoverOrderParams order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order parameters cannot be null");
                
            var result = await PlaceCoverOrder(new PlaceCoverOrderParams[] { order });
            return result != null && result.Length > 0 ? result[0] : null;
        }
        public virtual async Task<PlaceCoverOrderResult[]> PlaceCoverOrder(PlaceCoverOrderParams[] orders)
        {
            if (orders == null || orders.Length == 0)
                throw new ArgumentException("Orders array cannot be null or empty", nameof(orders));
                
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                if (order.StopLoss <= 0)
                    throw new ArgumentException("Stop loss required and must be greater than zero", nameof(order.StopLoss));

                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_COVER_ORDER;
                order.ProductCode = Constants.PRODUCT_CODE_CO;
            }

            return await ExecutePostAsync<PlaceCoverOrderResult[]>(_urls["order.place"], orders);
        }

        public virtual async Task<PlaceBracketOrderResult> PlaceBracketOrder(PlaceBracketOrderParams order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order parameters cannot be null");
                
            var result = await PlaceBracketOrder(new PlaceBracketOrderParams[] { order });
            return result != null && result.Length > 0 ? result[0] : null;
        }

        public virtual async Task<PlaceBracketOrderResult[]> PlaceBracketOrder(PlaceBracketOrderParams[] orders)
        {
            if (orders == null || orders.Length == 0)
                throw new ArgumentException("Orders array cannot be null or empty", nameof(orders));
                
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                if (order.StopLoss <= 0)
                    throw new ArgumentException("Stop loss required and must be greater than zero", nameof(order.StopLoss));
                if (order.Target <= 0)
                    throw new ArgumentException("Target required and must be greater than zero", nameof(order.Target));

                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_BRACKET_ORDER;
                order.ProductCode = Constants.PRODUCT_CODE_BO;
            }

            return await ExecutePostAsync<PlaceBracketOrderResult[]>(_urls["order.place"], orders);
        }

        protected virtual void PlaceOrderValidateRequiredArguments(PlaceRegularOrderParams order)
        {
            if (string.IsNullOrEmpty(order.TradingSymbol))
                throw new ArgumentNullException("Trading symbol required.");

            if (!order.InstrumentToken.HasValue)
                throw new ArgumentNullException("Instrument token required.");

            if (!order.Quantity.HasValue)
                throw new ArgumentNullException("Quantity required.");

            if (string.IsNullOrEmpty(order.TransactionType))
                throw new ArgumentNullException("Transaction type required.");

            if (string.IsNullOrEmpty(order.PriceType))
                throw new ArgumentNullException("Price type required.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="filePath"></param>
        /// <exception cref="DirectoryNotFoundException">File directory should exists.</exception>
        public virtual async Task SaveMasterContracts(string exchange, string filePath)
        {
            if (string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException(nameof(exchange), "Exchange cannot be null or empty");
                
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty");
                
            await DownloadMasterContract(exchange, (stream) =>
            {
                var fileStream = File.Create(filePath);
                stream.CopyTo(fileStream);
                fileStream.Close();
            });
        }

        protected virtual async Task<bool> DownloadMasterContract(string exchange, Action<Stream> processStream)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 30);
            httpClient.DefaultRequestHeaders.Clear();

            var url = _urls["contract.master.csv"].Replace("{EXCHANGE}", exchange.ToUpper());

            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                using (stream)
                {
                    processStream.Invoke(stream);
                }
            }

            return true;
        }

        public virtual async Task<BasketMarginResult> GetBasketMargin(List<BasketMarginItem> basketItems)
        {
            if (basketItems == null || basketItems.Count == 0)
                throw new ArgumentException("Basket items list cannot be empty.", nameof(basketItems));

            return await ExecutePostAsync<BasketMarginResult>(_urls["basket.margin"], basketItems);
        }

        public virtual async Task<IList<Contract>> GetMasterContracts(string exchange)
        {
            List<Contract> contracts = new List<Contract>(100000);

            await DownloadMasterContract(exchange, (stream) =>
            {
                var streamReader = new StreamReader(stream);
                using (var csv = new CsvReader(streamReader, true))
                {
                    while (csv.ReadNextRecord())
                    {
                        try
                        {
                            var contract = new Contract
                            {
                                Exchange = csv["Exch"],
                                ExchangeSegment = csv.HasHeader("Exchange Segment") ? csv["Exchange Segment"] : null,
                                InstrumentSymbol = csv["Symbol"],
                                InstrumentName = csv.HasHeader("Instrument Name") ? csv["Instrument Name"] : null,
                                InstrumentToken = int.Parse(csv["Token"]),
                                InstrumentType = csv.HasHeader("Instrument Type") ? csv["Instrument Type"] : null,
                                OptionType = csv.HasHeader("Option Type") ? csv["Option Type"] : null,
                                Strike = csv.HasHeader("Strike Price") && !string.IsNullOrEmpty(csv["Strike Price"]) ? decimal.Parse(csv["Strike Price"]) : 0.0m,
                                FormattedInstrumentName = csv.HasHeader("Formatted Ins Name") ? csv["Formatted Ins Name"] : null,
                                TradingSymbol = csv.HasHeader("Trading Symbol") ? csv["Trading Symbol"] : null,
                                Expiry = csv.HasHeader("Expiry Date") && !string.IsNullOrEmpty(csv["Expiry Date"]) ? DateTime.Parse(csv["Expiry Date"]) : default(DateTime?),
                                LotSize = csv.HasHeader("Lot Size") && !string.IsNullOrEmpty(csv["Lot Size"]) ? int.Parse(csv["Lot Size"]) : 0,
                                TickSize = csv.HasHeader("Tick Size") && !string.IsNullOrEmpty(csv["Tick Size"]) ? decimal.Parse(csv["Tick Size"]) : 0,
                            };

                            if (string.IsNullOrEmpty(contract.TradingSymbol))
                            {
                                contract.TradingSymbol = contract.InstrumentSymbol;
                            }

                            contracts.Add(contract);
                        }
                        catch (Exception ex)
                        {
                            if (_enableLogging) Utils.LogMessage(ex.ToString());
                        }
                    }
                }
            });

            return contracts;
        }

        public async Task<T> ExecutePostAsync<T>(string endpoint, object inputParams = null) where T : class
        {
            return await ExecuteAsync<T>(endpoint, inputParams, Method.Post);
        }
        
        public async Task<T> ExecuteGetAsync<T>(string endpoint, object inputParams = null) where T : class
        {
            return await ExecuteAsync<T>(endpoint, inputParams, Method.Get);
        }
        
        // Legacy methods for backward compatibility
        public T ExecutePost<T>(string endpoint, object inputParams = null) where T : class
        {
            return ExecuteAsync<T>(endpoint, inputParams, Method.Post).GetAwaiter().GetResult();
        }
        
        public T ExecuteGet<T>(string endpoint, object inputParams = null) where T : class
        {
            return ExecuteAsync<T>(endpoint, inputParams, Method.Get).GetAwaiter().GetResult();
        }

        protected async Task<T> ExecuteAsync<T>(string endpoint, object inputParams = null, Method method = Method.Get) where T : class
        {
            var request = new RestRequest(endpoint);

            if (inputParams != null)
                request.AddStringBody(Utils.Serialize(inputParams), ContentType.Json);

            var response = await _restClient.ExecuteAsync<T>(request, method);

            if (!string.IsNullOrEmpty(response.Content))
            {
                if (response.Content == "Unauthorized")
                {
                    throw new UnauthorizedAccessException(response.ErrorException?.ToString());
                }

                if (response.Content.Contains(Constants.API_RESPONSE_STATUS_Not_OK))
                {
                    var errorMessage = $"Error executing API request. Status: {response.StatusCode}-{response.ErrorMessage}";
                    if (_enableLogging)
                        Utils.LogMessage(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            if (!string.IsNullOrEmpty(response.Content) && response.Content.Contains(Constants.API_RESPONSE_STATUS_OK))
                return response.Data;
            else
            {
                var errorMessage = $@"
                        Status Code: {response.StatusCode}
                        Status Description: {response.StatusDescription}
                        Content: {response.Content}
                        Error Message: {response.ErrorMessage}
                        Error Exception: {response.ErrorException?.Message}";

                if (_enableLogging)
                    Utils.LogMessage(errorMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(errorMessage);

                return default(T);
            }
        }
        
        // Legacy method for backward compatibility
        protected T Execute<T>(string endpoint, object inputParams = null, Method method = Method.Get) where T : class
        {
            return ExecuteAsync<T>(endpoint, inputParams, method).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Disposes the resources used by the AliceBlue client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes the resources used by the AliceBlue client
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _restClient?.Dispose();
                _ticker?.Dispose();
            }
        }
    }
}