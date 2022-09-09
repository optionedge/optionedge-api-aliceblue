using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class PositionBookResult : BaseResponseResult
    {
        [JsonPropertyName("Exchange")]       
        public string Exchage { get; set; }

        [JsonPropertyName("Tsym")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("Netqty")]
        public int NetQuantity { get; set; }

        [JsonPropertyName("Netamt")]
        public decimal NetAmount { get; set; }

        [JsonPropertyName("Pcode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("MtoM")]
        public decimal M2M { get; set; }

        [JsonPropertyName("BEP")]
        public decimal BreakevenPointBreak { get; set; }

        [JsonPropertyName("LTP")]
        public decimal Ltp { get; set; }

        [JsonPropertyName("Instname")]
        public string InstrumentName { get; set; }

        [JsonPropertyName("Expdate")]
        public string Expiry { get; set; }

        [JsonPropertyName("Opttype")]
        public string OptionType { get; set; }

        [JsonPropertyName("Stikeprc")]
        public decimal Strike { get; set; }

        [JsonPropertyName("Buyavgprc")]
        public decimal BuyAveragePrice { get; set; }

        [JsonPropertyName("Sellavgprc")]
        public decimal SellAveragePrice { get; set; }

        [JsonPropertyName("Bqty")]
        public int QuantityBought { get; set; }

        [JsonPropertyName("Sqty")]
        public int SquareOffQuantity { get; set; }

        [JsonPropertyName("Fillbuyamt")]
        public decimal FillBuyAmount { get; set; }

        [JsonPropertyName("Fillsellamt")]
        public string FillSellAmount { get; set; }

        [JsonPropertyName("BLQty")]
        public decimal LotSize { get; set; }

        [JsonPropertyName("Token")]
        public string InstrumentToken { get; set; }

        /// <summary>
        /// Trading Symbol
        /// Why duplicate property?
        /// </summary>
        [JsonPropertyName("Symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("Exchangeseg")]
        public string ExchangeSegment { get; set; }

        [JsonPropertyName("Fillbuyqty")]
        public int FillBuyQuantity { get; set; }

        [JsonPropertyName("Fillsellqty")]
        public int FillSellQuantity { get; set; }

        [JsonPropertyName("s_NetQtyPosConv")]
        public string NetQuantityPositionConversion { get; set; }

        [JsonPropertyName("posflag")]
        public string PartialPositionConversionFlag { get; set; }

        [JsonPropertyName("sSqrflg")]
        public string SquareOffFlag { get; set; }

        [JsonPropertyName("discQty")]
        public string DisclosedQuantity { get; set; }

        [JsonPropertyName("PriceNumerator")]
        public int PriceNumerator { get; set; }

        [JsonPropertyName("GeneralNumerator")]
        public int GeneralNumerator { get; set; }

        [JsonPropertyName("PriceDenomenator")]
        public int PriceDenomenator { get; set; }

        [JsonPropertyName("GeneralDenomenator")]
        public int GeneralDenomenator { get; set; }

        [JsonPropertyName("companyname")]
        public string CompanyName { get; set; }

        [JsonPropertyName("realisedprofitloss")]
        public string RealisedProfitLoss { get; set; }

        [JsonPropertyName("unrealisedprofitloss")]
        public decimal UnrealisedProfitLoss { get; set; }

        [JsonPropertyName("Type")]
        public string PositionType { get; set; }

        [JsonPropertyName("Series")]
        public string Series { get; set; }

        [JsonPropertyName("netSellamt")]
        public decimal NetSellAmount { get; set; }

        [JsonPropertyName("netbuyamt")]
        public decimal NetBuyAmount { get; set; }

        [JsonPropertyName("netbuyqty")]
        public int NetBuyQty { get; set; }

        [JsonPropertyName("netsellqty")]
        public int NetSellQty { get; set; }

        [JsonPropertyName("CFBuyavgprc")]
        public decimal CFNetBuyAveragePrice { get; set; }

        [JsonPropertyName("CFsellavgprc")]
        public decimal CFNetSellAveragePrice { get; set; }

        [JsonPropertyName("CFbuyqty")]
        public int CFBuyQuantity { get; set; }

        [JsonPropertyName("CFsellqty")]
        public int CFSellQuantity { get; set; }

        [JsonPropertyName("FillbuyamtCF")]
        public decimal FillBuyAmountCF { get; set; }

        [JsonPropertyName("FillsellamtCF")]
        public decimal FillSellAmountCF { get; set; }

        [JsonPropertyName("actid")]
        public string AccountId { get; set; }

        [JsonPropertyName("NetBuyavgprc")]
        public decimal NetBuyAveragePrice { get; set; }

        [JsonPropertyName("NetSellavgprc")]
        public decimal NetSellAveragePrice { get; set; }
    }
}
