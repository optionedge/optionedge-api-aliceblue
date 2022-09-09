using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OptionEdge.API.AliceBlue.Records;

namespace OptionEdge.API.AliceBlue.Samples
{
    public class FeaturesDemo
    {
        // apiKey, userId, logging setting
        static Settings _settings;

        static IAliceBlue _aliceBlue;
        static Ticker _ticker;

        static string _cachedTokenFile = "cached_token.txt";

        public void Run()
        {
            try
            {
                // Read ApiKey, userId from Settings 
                _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));


                // ==========================
                // Initialize
                // ==========================

                // Create new instance of AliceBlue client library
                _aliceBlue = new AliceBlue(_settings.UserId, _settings.ApiKey, enableLogging: _settings.EnableLogging,
                    onAccessTokenGenerated: (accessToken) =>
                    {
                        // Store the generated access token to database or file store
                        // Token needs to be generated only once for the day

                        File.WriteAllText(_cachedTokenFile, accessToken);
                    }, cachedAccessTokenProvider: () =>
                    {
                        // Provide the saved token that will be used to making the REST calls.
                        // This method will be used when re-initializing the api client during the the day (eg after app restart etc)

                        // If token is invalid or not available, just return empty or null value
                        return File.Exists(_cachedTokenFile) ? File.ReadAllText(_cachedTokenFile) : null;
                    });


                // ==========================
                // Live Feeds Data Streaming
                // ==========================

                // Create Ticker instance
                // No need to provide the userId, apiKey, it will be automatically set
                _ticker = _aliceBlue.CreateTicker();

                // Setup event handlers
                _ticker.OnTick += _ticker_OnTick;
                _ticker.OnConnect += _ticker_OnConnect;
                _ticker.OnClose += _ticker_OnClose;
                _ticker.OnError += _ticker_OnError;
                _ticker.OnNoReconnect += _ticker_OnNoReconnect;
                _ticker.OnReconnect += _ticker_OnReconnect;

                // Connect the ticker to start receiving the live feeds
                // DO NOT FORGOT TO CONNECT else you will not receive any feed
                _ticker.Connect();

                // Once connected, subscribe to tokens to start receiving the feeds

                // Subscribe live feeds
                // Set feed mode to Quote (no depth data will be received)
                // Set feed mode to Full to get depth data as well
                // Token for multiple exchnages in one call
                _ticker.Subscribe(Constants.TICK_MODE_QUOTE,
                    new SubscriptionToken[]
                        {
                            new SubscriptionToken
                            {
                                Token = 26000,
                                Exchange = Constants.EXCHANGE_NSE
                            },
                            new SubscriptionToken
                            {
                                Token = 26009,
                                Exchange = Constants.EXCHANGE_NSE
                            },
                            new SubscriptionToken
                            {
                                Token = 35042,
                                Exchange = Constants.EXCHANGE_NFO
                            }
                        });

                // or subscribe to tokens for specific exchange
                _ticker.Subscribe(Constants.EXCHANGE_NSE, Constants.TICK_MODE_FULL, new int[] { 26000, 26009 });

                // To unscubscribe
                // tokens for multiple exchanges in one call
                _ticker.UnSubscribe(new SubscriptionToken[]
                {
                    new SubscriptionToken
                    {
                        Token = 35042,
                        Exchange = Constants.EXCHANGE_NFO
                    },
                    new SubscriptionToken
                    {
                        Token = 26000,
                        Exchange  = Constants.EXCHANGE_NSE
                    } 
                });

                // or unsubscribe tokens from specific exchange
                _ticker.UnSubscribe(Constants.EXCHANGE_NFO, new int[] { 26000, 26000 });

                // Ticker has built in re-connection mechanism
                // Once disconnectd it will try to reconnect, once connected, it will automatically subscribe to
                // previously subscribed tokens

                // Reconnection settings
                _ticker.EnableReconnect(interval: 5, retries: 20);
                // interval: interval between re-connection retries.
                // For every reconnect attempt OnReconnect event will be called

                // retries: number of retries before it give up reconnecting.
                // OnNoReconnect will be called if not able to reconnect



                // ==========================
                // Download Master Contracts
                // ==========================
                _aliceBlue.SaveMasterContracts(Constants.EXCHANGE_NFO, $"c:\\data\\master_contracts_{Constants.EXCHANGE_NFO}.csv");

                // or load Master Contracts into list
                var masterContracts = _aliceBlue.GetMasterContracts(Constants.EXCHANGE_NFO);


                // ==========================
                // Account Details
                // ==========================
                var accountDetails = _aliceBlue.GetAccountDetails();


                // ==========================
                // Funds
                // ==========================
                var funds = _aliceBlue.GetFunds();


                // ==========================
                // Historical Data
                // ==========================
                var historicalCandles = _aliceBlue.GetHistoricalData(
                    Constants.EXCHANGE_NFO,
                    36257,
                    DateTime.Parse("6-Sep-2022 9:30 AM"),
                    DateTime.Parse("8-Sep-2022 3:30 PM"),
                    Constants.HISTORICAL_DATA_RESOLUTION_1_MINUTE);



                // ==========================
                // Holdings
                // ==========================
                var holdings = _aliceBlue.GetHoldings();


                // ==========================
                // Open Interest
                // ==========================
                var openInterest = _aliceBlue.GetOpenInterest(new OpenInterestParams
                {
                    OpenInterestTokens = new[]
                    {
                        new OpenInterestToken
                        {
                            Exchange = Constants.EXCHANGE_NFO,
                            InstrumentToken = 36257
                        }
                    }                
                });

                // or pass tokens for specific exchange
                var openInterestNFO = _aliceBlue.GetOpenInterest(Constants.EXCHANGE_NFO, new[] { 12345, 43343, 5432 });


                // ==========================
                // Script Quote
                // ==========================
                var scriptQoute = _aliceBlue.GetScripQuote(Constants.EXCHANGE_NFO, 36257);



                // ==========================
                // Order Book
                // ==========================
                var orderBook = _aliceBlue.GetOrderBook();



                // ==========================
                // Trade Book
                // ==========================
                var tradeBook = _aliceBlue.GetTradeBook();



                // ==========================
                // Order History
                // ==========================
                var orderHistory = _aliceBlue.GetOrderHistory(orderNumber: "123456789");



                // ==========================
                // Day/net wise Position Book
                // ==========================
                var positionBookDayWise = _aliceBlue.GetPositionBookDayWise();
                var positionBookNetWise = _aliceBlue.GetPositionBookNetWise();



                // ==========================
                // Place Order - Regular
                // ==========================
                var placeRegularOrderResult = _aliceBlue.PlaceOrder(new PlaceRegularOrderParams
                {
                    Exchange = Constants.EXCHANGE_NFO,
                    OrderTag = "Test",
                    PriceType = Constants.PRICE_TYPE_MARKET,
                    ProductCode = Constants.PRODUCT_CODE_MIS,
                    Quantity = 25,
                    TransactionType = Constants.TRANSACTION_TYPE_BUY,
                    InstrumentToken = 46335,
                    TradingSymbol = "BANKNIFTY2290838000PE"
                });


                // ==========================
                // Place Order - Cover
                // ==========================
                var placeCoverOrderResult = _aliceBlue.PlaceCoverOrder(new PlaceCoverOrderParams
                {
                    Exchange = Constants.EXCHANGE_NSE,
                    OrderTag = "Test",
                    PriceType = Constants.PRICE_TYPE_LIMIT,
                    ProductCode = Constants.PRODUCT_CODE_MIS,
                    TransactionType = Constants.TRANSACTION_TYPE_BUY,
                    InstrumentToken = 2885,
                    TradingSymbol = "RELIANCE-EQ",
                    Quantity = 1,
                    Price = 2560m,
                    TriggerPrice = 2559m,
                    StopLoss = 2545m
                });



                // ==========================
                // Place Order - Bracket
                // ==========================
                var placeBracketOrderResult = _aliceBlue.PlaceBracketOrder(new PlaceBracketOrderParams
                {
                    Exchange = Constants.EXCHANGE_NSE,
                    OrderTag = "Test",
                    PriceType = Constants.PRICE_TYPE_LIMIT,
                    ProductCode = Constants.PRODUCT_CODE_MIS,
                    TransactionType = Constants.TRANSACTION_TYPE_BUY,
                    InstrumentToken = 2885,
                    TradingSymbol = "RELIANCE-EQ",
                    Quantity = 1,
                    Price = 2560m,
                    TriggerPrice = 2559m,
                    StopLoss = 2545m,
                    Target = 2590m,
                });



                // ==========================
                // Modify Order
                // ==========================         
                if (placeRegularOrderResult != null && placeRegularOrderResult.Status == Constants.STATUS_OK)
                {
                    Console.WriteLine($"Order executed. Order Number: {placeRegularOrderResult.OrderNumber}");

                    // Get the status of the order
                    var orderStatus = _aliceBlue.ModifyOrder(new ModifyOrderParams
                    {
                        Exchange = Constants.EXCHANGE_NFO,
                        OrderNumber = placeRegularOrderResult.OrderNumber,
                        Price = .5m,
                        PriceType = Constants.PRICE_TYPE_LIMIT,
                        ProductCode = Constants.PRODUCT_CODE_MIS,
                        Qty = 25,
                        TradingSymbol = "BANKNIFTY2290838000PE",
                        TransactionType = Constants.TRANSACTION_TYPE_SELL,
                    });
                }



                // ==========================
                // Cancel Order
                // ==========================      
                if (placeRegularOrderResult != null && placeRegularOrderResult.Status == Constants.STATUS_OK)
                {
                    Console.WriteLine($"Order executed. Order Number: {placeRegularOrderResult.OrderNumber}");

                    var orderStatus = _aliceBlue.CancelOrder(placeRegularOrderResult.OrderNumber);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }


        private static void _ticker_OnTick(Tick TickData)
        {
            Console.WriteLine(JsonConvert.SerializeObject(TickData));
        }

        private static void _ticker_OnReconnect()
        {
            Console.WriteLine("Ticker reconnecting.");
        }

        private static void _ticker_OnNoReconnect()
        {
            Console.WriteLine("Ticker not reconnected.");
        }

        private static void _ticker_OnError(string Message)
        {
            Console.WriteLine("Ticker error." + Message);
        }

        private static void _ticker_OnClose()
        {
            Console.WriteLine("Ticker closed.");
        }

        private static void _ticker_OnConnect()
        {
            Console.WriteLine("Ticker connected.");

            Thread.Sleep(2000);
            _ticker.Subscribe(Constants.TICK_MODE_FULL,
                new SubscriptionToken[]
                    {
                       new SubscriptionToken
                       {
                           Exchange = Constants.EXCHANGE_NSE,
                           Token = 26000
                       },
                       new SubscriptionToken
                       {
                           Exchange = Constants.EXCHANGE_NSE,
                           Token = 26009
                       },
                       new SubscriptionToken
                       {
                           Exchange = Constants.EXCHANGE_NFO,
                           Token = 35042
                       },
                    });
        }
    }
}
