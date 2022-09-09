using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class ScriptQuoteResult : BaseResponseResult
    {
        [JsonPropertyName("optiontype")]       
        public string OptionType { get; set; }

        [JsonPropertyName("SQty")]
        public int SellQty { get; set; }

        [JsonPropertyName("vwapAveragePrice")]
        public string VWAPAveragePrice { get; set; }

        [JsonPropertyName("LTQ")]
        public int LastTradedQty { get; set; }

        [JsonPropertyName("LTP")]
        public string LastTradedPrice { get; set; }      

        [JsonPropertyName("DecimalPrecision")]
        public decimal DecimalPrecision { get; set; }

        [JsonPropertyName("openPrice")]
        public decimal OpenPrice { get; set; }

        [JsonPropertyName("BRate")]
        public decimal BaseRate { get; set; }

        [JsonPropertyName("defmktproval")]
        public string DefaultMarketProtectionValue { get; set; }

        [JsonPropertyName("BQty")]
        public int BuyQty { get; set; }

        [JsonPropertyName("symbolname")]
        public string SymbolName { get; set; }

        [JsonPropertyName("noMktPro")]
        public string noMktPro { get; set; }

        [JsonPropertyName("LTT")]
        public string LTT { get; set; }

        [JsonPropertyName("mktpro")]
        public string mktpro { get; set; }

        [JsonPropertyName("TickSize")]
        public decimal TickSize { get; set; }

        [JsonPropertyName("Multiplier")]
        public int Multiplier { get; set; }

        [JsonPropertyName("strikeprice")]
        public string Strike { get; set; }

        [JsonPropertyName("TotalSell")]
        public string TotalSell { get; set; }

        [JsonPropertyName("High")]
        public decimal High { get; set; }

        [JsonPropertyName("BodLotQty")]
        public int BodLotQty { get; set; }

        [JsonPropertyName("yearlyHighPrice")]
        public decimal YearlyHighPrice { get; set; }

        [JsonPropertyName("yearlyLowPrice")]
        public decimal YearlyLowPrice { get; set; }

        [JsonPropertyName("exchFeedTime")]
        public string ExchangeFeedTime { get; set; }

        [JsonPropertyName("PrvClose")]
        public decimal PreviousClose { get; set; }

        [JsonPropertyName("SRate")]
        public decimal SRate { get; set; }

        [JsonPropertyName("Change")]
        public decimal Change { get; set; }

        [JsonPropertyName("Series")]
        public string Series { get; set; }

        [JsonPropertyName("TotalBuy")]
        public string TotalBuyQty { get; set; }

        [JsonPropertyName("Low")]
        public decimal Low { get; set; }

        [JsonPropertyName("UniqueKey")]
        public string UniqueKey { get; set; }

        [JsonPropertyName("PerChange")]
        public decimal PercentageChange { get; set; }

        [JsonPropertyName("companyname")]
        public string CompanyName { get; set; }

        [JsonPropertyName("TradeVolume")]
        public int TradeVolume { get; set; }

        [JsonPropertyName("TSymbl")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("Exp")]
        public string Exp { get; set; }

        [JsonPropertyName("LTD")]
        public string LTD { get; set; }

        [OnError]
        public void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
