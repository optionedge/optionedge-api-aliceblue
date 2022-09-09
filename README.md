# .Net library for AliceBlue REST API 
Client library to communicate with [AliceBlue v2 REST API](https://v2api.aliceblueonline.com/introduction).

OptionEdge client library provides simpler interface to connect to AliceBlue REST Api and live streaming services.

### **READ THIS CAREFULLY**
Current [AliceBlue V2 Api](https://v2api.aliceblueonline.com/introduction) documentation and [Postman collection](https://v2api.aliceblueonline.com/introduction) provided has many inconsistencies. Abbreviated & inconsistent field names & data types are used many places. API seems to be still under development. I have tried my best to abstract away all the lower level details and provided more readable & consistent interface to interact with the Api. However, current REST Api is still not stable and Api parameters, data types & rest endpoint might change.

Currently this library is in beta stage, please expect the methods, parameters to be changed based on the feedback.
## Refer to my channel on API Usage Guide [ProfTheta - Your Guide to Options](https://www.youtube.com/channel/UChp2hjl-OgGpHKCrwJPohEQ) (to be posted)

## Please refer this Algo Trading Course on Youtube
[Algo Trading Course](https://www.youtube.com/watch?v=o_WqyKSURr8) 


## Requirements

This library targets **netstandard2.0** and can be used with .**Net Core 2.0 and above** & **.Net Framework 4.6 and above**.

## Getting Started

You have to first generate the Api Key from [ANT Web application](https://a3.aliceblueonline.com/). Please login to ANT web app and then click on the Generate API Key button.

**Log into ANT Web** &raquo; **Apps** &raquo; **API Key** &raquo; **Generate/Regenerate API Key**

Please note down the Api key & your Alice Blue User Id. You will be using the Api key & user id to initialize the client library. 

It is a requirement from AliceBlue that you login to your account (ANT web) everyday before using the Api/Client library else REST calls will fail.

## Troubleshooting
enable logging flag is set to true during the initialization, client library will log all the error/info messages to the console. This will help to troubleshoot any issues while integrating library with your project. You can disable this flag in production.

## Install library

```
Install-Package OptionEdge.API.AliceBlue -Version 1.0.0.2-beta
```

## Sample project
Please refer the sample project `FeaturesDemo.cs` which demonstrate the capabilities of this library. 

## Getting Started guide on Youtube
Please refer this [Youtube](https://www.youtube.com/channel/UChp2hjl-OgGpHKCrwJPohEQ) (posting soon) video to get started using the library by creating a new project and calling the provided methods from the library for placing orders, getting order & trade history, historical data & live quotes. 

## Initialize

```csharp
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
```

## Live Feeds Data Streaming
```csharp
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

>1. Only Day and Minute data will be available. Other resolutions can be calculated on their own, based user preferences using these resolutions.
>2. Historical data API will be available from 5:30 PM (evening) to 8 AM (Next day morning) on weekdays (Monday to Friday). Historical data will not be available during market hours
>3. Historical data API will be available fully during weekends and holidays.
>4. For NSE segment, 2 years of historical data will be provided.
>5. For NFO, CDS and MCX segments, current expiry data will be provided.
>6. BSE, BCD and BFO Chart data will be added later.

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

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
