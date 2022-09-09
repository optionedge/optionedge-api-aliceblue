using OptionEdge.API.AliceBlue.Records;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OptionEdge.API.AliceBlue
{
    public interface IAliceBlue
    {
        CancelOrderResult CancelOrder(string orderNumber);
        Ticker CreateTicker();
        T ExecuteGet<T>(string endpoint, object inputParams = null) where T : class;
        T ExecutePost<T>(string endpoint, object inputParams = null) where T : class;
        ExitBracketOrderResult ExitBracketOrder(ExitBracketOrderParams exitBracketOrderParams);
        ExitCoverOrderResult ExitCoverOrder(ExitCoverOrderParams exitCoverOrder);
        FundsResult[] GetFunds();
        HistoryDataResult GetHistoricalData(HistoryDataParams historyDataParams);
        HistoryDataResult GetHistoricalData(string exchange, int instrumentToken, DateTime from, DateTime to, string resolution);
        HoldingsResult GetHoldings();
        Task<IList<Contract>> GetMasterContracts(string exchange);
        OpenInterestResult[] GetOpenInterest(string exchange, int[] tokens);
        OpenInterestResult[] GetOpenInterest(OpenInterestParams tokens);
        OrderBookResult[] GetOrderBook();
        OrderHistoryResult[] GetOrderHistory(string orderNumber);
        PositionBookResult[] GetPositionBookDayWise();
        PositionBookResult[] GetPositionBookNetWise();
        AccountDetails GetAccountDetails();
        ScriptQuoteResult GetScripQuote(string exchange, int instrumentToken);
        TradeBookResult[] GetTradeBook();
        ModifyOrderResult ModifyOrder(ModifyOrderParams modifyOrderParams);
        PlaceBracketOrderResult PlaceBracketOrder(PlaceBracketOrderParams order);
        PlaceBracketOrderResult[] PlaceBracketOrder(PlaceBracketOrderParams[] orders);
        PlaceCoverOrderResult PlaceCoverOrder(PlaceCoverOrderParams order);
        PlaceCoverOrderResult[] PlaceCoverOrder(PlaceCoverOrderParams[] orders);
        PlaceBracketOrderResult PlaceOrder(PlaceBracketOrderParams order);
        PlaceBracketOrderResult[] PlaceOrder(PlaceBracketOrderParams[] orders);
        PlaceCoverOrderResult PlaceOrder(PlaceCoverOrderParams order);
        PlaceCoverOrderResult[] PlaceOrder(PlaceCoverOrderParams[] orders);
        PlaceRegularOrderResult PlaceOrder(PlaceRegularOrderParams order);
        PlaceRegularOrderResult[] PlaceOrder(PlaceRegularOrderParams[] orders);
        void SaveMasterContracts(string exchange, string filePath);
        SquareOffPositionResult SquareOffPosition(SquareOffPositionParams squareOffPositionParams);
    }
}