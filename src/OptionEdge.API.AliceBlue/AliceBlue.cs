using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using OptionEdge.API.AliceBlue.Records;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;

namespace OptionEdge.API.AliceBlue
{
    public class AliceBlue : IAliceBlue
    {
        string _baseUrl = "https://a3.aliceblueonline.com/rest/AliceBlueAPIService/api";
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

            ["history"] = "/chart/history",

            ["scrip.quote"] = "/ScripDetails/getScripQuoteDetails",
            ["scrip.open.interest"] = "/marketWatch/scripsMW",

            ["ws.session.create"] = "/ws/createSocketSess",
            ["ws.session.invalidate"] = "/ws/invalidateSocketSess"
        };

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

            _restClient = new RestClient(options)
            {
                Authenticator = new AliceBlueAuthenticator(_userId, _apiKey, _baseUrl, _urls["auth.encryption.key"], _urls["auth.session.id"], enableLogging, (accessToken) =>
                {
                    _accessToken = accessToken;
                    onAccessTokenGenerated?.Invoke(accessToken);
                }, cachedAccessTokenProvider)
            };
        }

        public Ticker CreateTicker()
        {
            CreateWebsocketSession();

            return new Ticker(_userId, GetWebsocketAccessToken(), socketUrl: _websocketUrl, debug: _enableLogging);
        }

        private void CreateWebsocketSession()
        {
            try
            {
                var response = ExecutePost<WebsocketSessionResult>(
                    _urls["ws.session.invalidate"],
                    new WebsocketAccessTokenParams
                    {
                        LoginType = "API"
                    });

                response = ExecutePost<WebsocketSessionResult>(
                    _urls["ws.session.create"],
                    new WebsocketAccessTokenParams
                    {
                        LoginType = "API"
                    });
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    Utils.LogMessage($"Error creating websocket session.{ex.Message}");
            }
        }

        private string GetWebsocketAccessToken()
        {
            return Utils.GetSHA256(Utils.GetSHA256(_accessToken));
        }

        public FundsResult[] GetFunds()
        {
            return ExecuteGet<FundsResult[]>(_urls["funds.limits"]);
        }

        // Not working - making it private
        public SquareOffPositionResult SquareOffPosition(SquareOffPositionParams squareOffPositionParams)
        {
            return ExecutePost<SquareOffPositionResult>(_urls["square.off.position"], squareOffPositionParams);
        }

        public OrderHistoryResult[] GetOrderHistory(string orderNumber)
        {
            return ExecutePost<OrderHistoryResult[]>(_urls["order.history"], new OrderHistoryParams { OrderNumber = orderNumber });
        }

        public OrderBookResult[] GetOrderBook()
        {
            return ExecuteGet<OrderBookResult[]>(_urls["order.book"]);
        }

        public TradeBookResult[] GetTradeBook()
        {
            return ExecuteGet<TradeBookResult[]>(_urls["trade.book"]);
        }

        public HistoryDataResult GetHistoricalData(HistoryDataParams historyDataParams)
        {
            return ExecutePost<HistoryDataResult>(_urls["history"], historyDataParams);
        }

        public HistoryDataResult GetHistoricalData(string exchange, int instrumentToken, DateTime from, DateTime to, string resolution)
        {
            HistoryDataParams historyDataParams = new HistoryDataParams
            {
                Exchange = exchange,
                InstrumentToken = instrumentToken,
                From = ((DateTimeOffset)from).ToUnixTimeMilliseconds(),
                To = ((DateTimeOffset)to).ToUnixTimeMilliseconds(),
                Resolution = resolution
            };

            return GetHistoricalData(historyDataParams);
        }

        public ModifyOrderResult ModifyOrder(ModifyOrderParams modifyOrderParams)
        {
            return ExecutePost<ModifyOrderResult>(_urls["order.modify"], modifyOrderParams);
        }

        public CancelOrderResult CancelOrder(string orderNumber)
        {
            return ExecutePost<CancelOrderResult>(_urls["order.cancel"], new CancelOrderParams
            {
                OrderNumber = orderNumber
            });
        }

        public ExitBracketOrderResult ExitBracketOrder(ExitBracketOrderParams exitBracketOrderParams)
        {
            return ExecutePost<ExitBracketOrderResult>(_urls["order.bracket.exit"], exitBracketOrderParams);
        }

        public ExitCoverOrderResult ExitCoverOrder(ExitCoverOrderParams exitCoverOrder)
        {
            return ExecutePost<ExitCoverOrderResult>(_urls["order.cover.exit"], exitCoverOrder);
        }

        public OpenInterestResult[] GetOpenInterest(string exchange, int[] tokens)
        {
            var openInterestParams = new OpenInterestParams
            {
                OpenInterestTokens = tokens.Select(token => new OpenInterestToken
                {
                    Exchange = exchange,
                    InstrumentToken = token
                }).ToArray()
            };
       
            return GetOpenInterest(openInterestParams);
        }

        public OpenInterestResult[] GetOpenInterest(OpenInterestParams tokens)
        {
            var openInterestParamsInternal = new OpenInterestParamsInternal
            {
                OpenInterestTokens = tokens.OpenInterestTokens,
                UserId = _userId,
            };

            return ExecutePost<OpenInterestResult[]>(_urls["scrip.open.interest"], openInterestParamsInternal);
        }

        public AccountDetails GetAccountDetails()
        {
            return ExecuteGet<AccountDetails>(_urls["profile.account"]);
        }

        public PositionBookResult[] GetPositionBookDayWise()
        {
            return GetPosition(Constants.POSITION_DAYWISE);
        }
        public PositionBookResult[] GetPositionBookNetWise()
        {
            return GetPosition(Constants.POSITION_NETWISE);
        }

        private PositionBookResult[] GetPosition(string retentionType)
        {
            return ExecutePost<PositionBookResult[]>(
                _urls["portfolio.position.book"],
                new PositionBookParams
                {
                    RetentionType = retentionType
                });
        }
        public ScriptQuoteResult GetScripQuote(string exchange, int instrumentToken)
        {
            var res = ExecutePost<ScriptQuoteResult>(
                _urls["scrip.quote"],
                new ScriptQuoteParams
                {
                    Exchange = exchange.ToUpper(),
                    InstrumentToken = instrumentToken
                });

            return res;
        }

        public HoldingsResult GetHoldings()
        {
            return ExecuteGet<HoldingsResult>(_urls["portfolio.holdings"]);
        }

        public PlaceRegularOrderResult PlaceOrder(PlaceRegularOrderParams order)
        {
            var placeOrderResult = PlaceOrder(new PlaceRegularOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public PlaceCoverOrderResult PlaceOrder(PlaceCoverOrderParams order)
        {
            var placeOrderResult = PlaceOrder(new PlaceCoverOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public PlaceBracketOrderResult PlaceOrder(PlaceBracketOrderParams order)
        {
            var placeOrderResult = PlaceOrder(new PlaceBracketOrderParams[] { order });
            return placeOrderResult != null ? placeOrderResult[0] : null;
        }

        public PlaceCoverOrderResult[] PlaceOrder(PlaceCoverOrderParams[] orders)
        {
            return PlaceCoverOrder(orders);
        }

        public PlaceBracketOrderResult[] PlaceOrder(PlaceBracketOrderParams[] orders)
        {
            return PlaceBracketOrder(orders);
        }

        public PlaceRegularOrderResult[] PlaceOrder(PlaceRegularOrderParams[] orders)
        {
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                if (string.IsNullOrEmpty(order.ProductCode))
                    throw new ArgumentNullException("Product code required.");

                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_REGULAR;
            }

            return ExecutePost<PlaceRegularOrderResult[]>(_urls["order.place"], orders);
        }

        public PlaceCoverOrderResult PlaceCoverOrder(PlaceCoverOrderParams order)
        {
            return PlaceCoverOrder(new PlaceCoverOrderParams[] { order })[0];
        }
        public PlaceCoverOrderResult[] PlaceCoverOrder(PlaceCoverOrderParams[] orders)
        {
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                //if (order.TrailingStopLoss <= 0)
                //    throw new ArgumentNullException("Trailing stop loss required.");
                if (order.StopLoss <= 0)
                    throw new ArgumentNullException("Stop loss required.");

                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_COVER_ORDER;
                order.ProductCode = Constants.PRODUCT_CODE_CO;
            }

            return ExecutePost<PlaceCoverOrderResult[]>(_urls["order.place"], orders);
        }

        public PlaceBracketOrderResult PlaceBracketOrder(PlaceBracketOrderParams order)
        {
            return PlaceBracketOrder(new PlaceBracketOrderParams[] { order })[0];
        }

        public PlaceBracketOrderResult[] PlaceBracketOrder(PlaceBracketOrderParams[] orders)
        {
            foreach (var order in orders)
            {
                PlaceOrderValidateRequiredArguments(order);

                //if (order.TrailingStopLoss <= 0)
                //    throw new ArgumentNullException("Trailing stop loss required.");
                if (order.StopLoss <= 0)
                    throw new ArgumentNullException("Stop loss required.");
                if (order.Target <= 0)
                    throw new ArgumentNullException("Target required.");


                if (string.IsNullOrEmpty(order.Exchange))
                    order.Exchange = Constants.EXCHANGE_NFO;

                if (string.IsNullOrEmpty(order.RetentionType))
                    order.RetentionType = Constants.RETENTION_TYPE_DAY;

                if (string.IsNullOrEmpty(order.PriceType))
                    order.PriceType = Constants.PRICE_TYPE_LIMIT;

                order.Complexty = Constants.ORDER_COMPLEXITY_BRACKET_ORDER;
                order.ProductCode = Constants.PRODUCT_CODE_BO;
            }

            return ExecutePost<PlaceBracketOrderResult[]>(_urls["order.place"], orders);
        }

        private void PlaceOrderValidateRequiredArguments(PlaceRegularOrderParams order)
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
        public void SaveMasterContracts(string exchange, string filePath)
        {
            var result = DownloadMasterContract(exchange, (stream) =>
             {
                 var fileStream = File.Create(filePath);
                 stream.CopyTo(fileStream);
                 fileStream.Close();
             }).Result;
        }

        private async Task<bool> DownloadMasterContract(string exchange, Action<Stream> processStream)
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

        public async Task<IList<Contract>> GetMasterContracts(string exchange)
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
                            contracts.Add(new Contract
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
                            });
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

        public T ExecutePost<T>(string endpoint, object inputParams = null) where T : class
        {
            return Execute<T>(endpoint, inputParams, Method.Post);
        }
        public T ExecuteGet<T>(string endpoint, object inputParams = null) where T : class
        {
            return Execute<T>(endpoint, inputParams, Method.Get);
        }

        protected T Execute<T>(string endpoint, object inputParams = null, Method method = Method.Get) where T : class
        {
            var request = new RestRequest(endpoint);

            if (inputParams != null)
                request.AddStringBody(Utils.Serialize(inputParams), ContentType.Json);

            var response = _restClient.ExecuteAsync<T>(request, method).Result;

            if (response != null && !string.IsNullOrEmpty(response.ErrorMessage) && _enableLogging)
                Utils.LogMessage($"Error executing api request. Status: {response.StatusCode}-{response.ErrorMessage}");


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
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

                return default(T);
            }
        }
    }
}