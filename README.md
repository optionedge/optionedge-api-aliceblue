# .Net library for AliceBlue REST API 
Client library to communicate with [AliceBlue v2 REST API](https://v2api.aliceblueonline.com/introduction).

OptionEdge client library provides simpler interface to connect to AliceBlue REST Api and live streaming services.

# Disclaimer
```
This software is an unofficial client libray for AliceBlue Api V2 and 
is not affiliated with / endorsed or approved by AliceBlue in any way.

This is purely an enthusiast program intended for educational purposes only and 
is not financial advice.

By downloading this software you acknowledge that you are using this 
at your own risk and that I am is not responsible for any damages that 
may occur to you due to the usage or installation of this program
```

## Refer to this Youtube video for API usage & integration guide  [ProfTheta - Your Guide to Options](https://www.youtube.com/watch?v=ncjVPPeSQ88)

[![Watch the video](https://img.youtube.com/vi/ncjVPPeSQ88/mqdefault.jpg)](https://www.youtube.com/watch?v=ncjVPPeSQ88)


## Refer to this video for free course on Algo Trading using broker APIs
[Algo Trading Course](https://www.youtube.com/watch?v=o_WqyKSURr8)


## Requirements

This library targets `netstandard2.0` and can be used with `.Net Core 2.0 and above` & `.Net Framework 4.6 and above`.

## Getting Started

You have to first generate the Api Key from [ANT Web application](https://a3.aliceblueonline.com/). Please login to ANT web app and then click on the Generate API Key button.

**Log into ANT Web** &raquo; **Apps** &raquo; **API Key** &raquo; **Generate/Regenerate API Key**

Please note down the Api key & your Alice Blue User Id. You will be using the Api key & user id to initialize the client library. 

**NOTE: User should Login through Web (a3.aliceblueonline.com) or Single Sign On (SSO) or Mobile at least once in a day, before connecting the API**

## Troubleshooting
While creating the instance of Alice Blue Api Client set enable logging parameter as true. Client library will log all the error/info messages to the console. This will help to troubleshoot any issues while integrating library with your project. You can disable this flag in production.

## Install library

```
Install-Package OptionEdge.API.AliceBlue -Version 1.0.2
```

## Sample project
Please refer the sample `FeaturesDemo.cs` class in `OptionEdge.API.AliceBlue.Samples' project which demonstrate the capabilities of this library. 

## Getting Started guide on Youtube
Please refer this [Youtube](https://www.youtube.com/channel/UChp2hjl-OgGpHKCrwJPohEQ) video to get started using the library by creating a new project and calling the provided methods from the library for placing orders, getting order & trade history, historical data & live quotes. 

## Import namespaces
```csharp
using OptionEdge.API.AliceBlue;
using OptionEdge.API.AliceBlue.Records;
```

## Declare variables
```csharp
AliceBlue _aliceBlue;
Ticker _ticker;

string _cachedTokenFile = $"cached_token_{DateTime.Now.ToString("dd_mmm_yyyy")}.txt";
```

## Initialize

```csharp
       
// Create new instance of AliceBlue client library
var_aliceBlue = new AliceBlue(_settings.UserId, _settings.ApiKey, enableLogging: _settings.EnableLogging,
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
```

## Live Feeds Data Streaming
```csharp
// Create Ticker instance
// No need to provide the userId, apiKey, it will be automatically set
var _ticker = _aliceBlue.CreateTicker();

// Setup event handlers
_ticker.OnTick += _ticker_OnTick;
_ticker.OnConnect += _ticker_OnConnect;
_ticker.OnClose += _ticker_OnClose;
_ticker.OnError += _ticker_OnError;
_ticker.OnNoReconnect += _ticker_OnNoReconnect;
_ticker.OnReconnect += _ticker_OnReconnect;
_ticker.OnReady += _ticker_OnReady;

// Connect the ticker to start receiving the live feeds
// DO NOT FORGOT TO CONNECT else you will not receive any feed
_ticker.Connect();
```

### Subscribe for live feeds
```csharp
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
```


```csharp
// or subscribe to tokens for specific exchange
_ticker.Subscribe(Constants.EXCHANGE_NSE, Constants.TICK_MODE_FULL, new int[] { 26000, 26009 });
```

### Unsubscribe from live feeds
```csharp
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
```

```csharp
// or unsubscribe tokens from specific exchange
_ticker.UnSubscribe(Constants.EXCHANGE_NFO, new int[] { 26000, 26000 });
```

### Auto Reconnect
```csharp
// Ticker has built in re-connection mechanism
// Once disconnectd it will try to reconnect, once connected, it will automatically subscribe to
// previously subscribed tokens

// Reconnection settings
_ticker.EnableReconnect(interval: 5, retries: 20);
// interval: interval between re-connection retries.
// For every reconnect attempt OnReconnect event will be called

// retries: number of retries before it give up reconnecting.
// OnNoReconnect will be called if not able to reconnect
```

## Download Master Contracts
```csharp
_aliceBlue.SaveMasterContracts(Constants.EXCHANGE_NFO, $"c:\\data\\master_contracts_{Constants.EXCHANGE_NFO}.csv");

// or load Master Contracts into list
var masterContracts = _aliceBlue.GetMasterContracts(Constants.EXCHANGE_NFO);
```

## Account Details
```csharp
var accountDetails = _aliceBlue.GetAccountDetails();
```

## Funds
```csharp
var funds = _aliceBlue.GetFunds();
```


## Historical Data

```csharp
var historicalCandles = _aliceBlue.GetHistoricalData(
    Constants.EXCHANGE_NFO,
    36257,
    DateTime.Parse("6-Sep-2022 9:30 AM"),
    DateTime.Parse("8-Sep-2022 3:30 PM"),
    Constants.HISTORICAL_DATA_RESOLUTION_1_MINUTE);
```

## Holdings
```csharp
var holdings = _aliceBlue.GetHoldings();
```

## Open Interest
```csharp
var openInterest = _aliceBlue.GetOpenInterest(new OpenInterestParams
{
    OpenInterestTokens = new[]
    {
        new OpenInterestToken
        {
            Exchange = Constants.EXCHANGE_NFO,
            InstrumentToken = 36257
        }
    },
    UserId = _settings.UserId
});
```

```csharp
// or pass tokens for specific exchange
var openInterestNFO = _aliceBlue.GetOpenInterest(Constants.EXCHANGE_NFO, new[] { 12345, 43343, 5432 });
```

## Scrip Quote
```csharp
var scripQuote = _aliceBlue.GetScripQuote(Constants.EXCHANGE_NFO, 36257);
```

## Order Book
```csharp
var orderBook = _aliceBlue.GetOrderBook();
```

## Trade Book
```csharp
var tradeBook = _aliceBlue.GetTradeBook();
```

## Order History
```csharp
var orderHistory = _aliceBlue.GetOrderHistory(orderNumber: "123456789");
```

## Day/net wise Position Book
```csharp
var positionBookDayWise = _aliceBlue.GetPositionBookDayWise();
var positionBookNetWise = _aliceBlue.GetPositionBookNetWise();
```

## Place Order - Regular
```csharp
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
```

## Place Order - Cover
```csharp
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
```

## Place Order - Bracket
```csharp
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
```

## Modify Order
```csharp
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
```


## Cancel Order
```csharp
if (placeRegularOrderResult != null && placeRegularOrderResult.Status == Constants.STATUS_OK)
{
    Console.WriteLine($"Order executed. Order Number: {placeRegularOrderResult.OrderNumber}");

    var orderStatus = _aliceBlue.CancelOrder(placeRegularOrderResult.OrderNumber);
}
```

## Web Socket event handlers
```csharp
private void _ticker_OnReady()
{
    Console.WriteLine("Socket connection authenticated. Ready to live stream feeds.");
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

    _ticker.Subscribe(new SubscriptionToken[]
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
    }, Constants.TICK_MODE_FULL);
}
```

# Links
- [AliceBlue V2 API Documentation](https://v2api.aliceblueonline.com/introduction)
- [AliceBlue V2 API Postman Collection](https://v2api.aliceblueonline.com/Aliceblue.postman_collection.json)
- [AliceBlue Ant Web](https://a3.aliceblueonline.com/)
- [ProfTheta Twitter @ProfTheta21](https://twitter.com/ProfTheta21)
- [ProfTheta Youtube Channel](https://www.youtube.com/channel/UChp2hjl-OgGpHKCrwJPohEQ)

# Credits
- Websocket code reference is based on Zerodha Kite Connect .Net Client Library 

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
