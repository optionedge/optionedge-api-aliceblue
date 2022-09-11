using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class OpenInterestResult : BaseResponseResult
    {
        [JsonPropertyName("tcksize")]
        public decimal TickSize { get; set; }

        [JsonPropertyName("openinterest")]
        public decimal OpenInterest { get; set; }

        [JsonPropertyName("BestSellPrice")]
        public decimal BestSellPrice { get; set; }

        [JsonPropertyName("optiontype")]
        public string OptionType { get; set; }

        [JsonPropertyName("ExchSeg")]
        public string ExchangeSegment { get; set; }

        [JsonPropertyName("defmktproval")]
        public string DefaultMarketProtectionValue { get; set; }

        [JsonPropertyName("BestBuySize")]
        public string BestBuySize { get; set; }

        [JsonPropertyName("noMktPro")]
        public string noMktPro { get; set; }

        [JsonPropertyName("CombinedSymbol")]
        public string CombinedSymbol { get; set; }

        [JsonPropertyName("symbolname")]
        public string SymbolName { get; set; }

        [JsonPropertyName("mktpro")]
        public string mktpro { get; set; }

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("lasttradedtime")]
        public string LastTradedTime { get; set; }

        [JsonPropertyName("BestSellSize")]
        public string BestSellSize { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("averageprice")]
        public decimal AveragePrice { get; set; }

        [JsonPropertyName("lasttradedquantity")]
        public int LastTradedQty { get; set; }

        [JsonPropertyName("strikeprice")]
        public decimal Strike { get; set; }

        [JsonPropertyName("close")]
        public decimal Close { get; set; }

        [JsonPropertyName("Expiry")]
        public string Expiry { get; set; }

        [JsonPropertyName("TradSym")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("spotprice")]
        public string SpotPrice { get; set; }

        [JsonPropertyName("BestBuyPrice")]
        public decimal BestBuyPrice { get; set; }

        [JsonPropertyName("multiplier")]
        public decimal Multiplier { get; set; }

        [JsonPropertyName("totalbuyqty")]
        public string TotalBuyQty { get; set; }

        [JsonPropertyName("totalsellqty")]
        public string TotalSellQty { get; set; }

        [JsonPropertyName("ltp")]
        public decimal Ltp { get; set; }

        [JsonPropertyName("Change")]
        public decimal Change { get; set; }

        [JsonPropertyName("decimalPrecision")]
        public decimal DecimalPrecision { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("Exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("UniqueKey")]
        public string UniqueKey { get; set; }

        [JsonPropertyName("corporateaction")]
        public string CorporateAction { get; set; }

        [JsonPropertyName("companyname")]
        public string CompanyName { get; set; }

        [JsonPropertyName("PerChange")]
        public decimal PercentChange { get; set; }

        [JsonPropertyName("Instrument")]
        public string Instrument { get; set; }

        [JsonPropertyName("TradeVolume")]
        public string TradeVolume { get; set; }

        [JsonPropertyName("bodlot")]
        public decimal LotSize { get; set; }

        [JsonPropertyName("maxQty")]
        public int MaxQty { get; set; }

        [JsonPropertyName("open")]
        public decimal Open { get; set; }

        [JsonPropertyName("minQty")]
        public int MinQty { get; set; }

        [JsonPropertyName("discQty")]
        public string DisclosedQty { get; set; }

        [JsonPropertyName("expdate")]
        public string ExpDate { get; set; }
    }
}
