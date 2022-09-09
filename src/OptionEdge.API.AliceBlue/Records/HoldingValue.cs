using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HoldingValue
    {
        [JsonPropertyName("Bsetsym")]
        public string BseTradingSymbol { get; set; }

        [JsonPropertyName("Nsetsym")]
        public string NseTradingSymbol { get; set; }

        [JsonPropertyName("Mcxsxcmsym")]
        public string MCXSXCMTradingSymbol { get; set; }

        [JsonPropertyName("Csetsym")]
        public string CSETradingSymbol { get; set; }

        [JsonPropertyName("Ysxtsym")]
        public string YSXTradingSymbol { get; set; }

        [JsonPropertyName("Exch1")]
        public string Exchange1 { get; set; }

        [JsonPropertyName("Exch2")]
        public string Exchange2 { get; set; }

        [JsonPropertyName("Exch3")]
        public string Exchange3 { get; set; }

        [JsonPropertyName("Exch4")]
        public string Exchange4 { get; set; }

        [JsonPropertyName("Exch5")]
        public string Exchange5 { get; set; }

        /// <summary>
        /// CNC Used Quantity.Volume of trade is the total used quantity of shares or contracts 
        /// traded for a specified security. It can be measured on any type of 
        /// security traded during a trading day.
        /// </summary>
        [JsonPropertyName("Usedqty")]
        public int UsedQuantity { get; set; }

        [JsonPropertyName("Holdqty")]
        public int HoldingQuantity { get; set; }

        [JsonPropertyName("Ltp")]
        public int Ltp { get; set; }

        [JsonPropertyName("Btst")]
        public int BTST { get; set; }

        [JsonPropertyName("Ttrind")]
        public string TTIndicator { get; set; }

        [JsonPropertyName("Scripcode")]
        public string ScripCode { get; set; }

        [JsonPropertyName("Series")]
        public string Series { get; set; }

        [JsonPropertyName("Pcode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("Coltype")]
        public string CollateralType { get; set; }

        [JsonPropertyName("HUqty")]
        public int HoldingUpdateQuantity { get; set; }

        [JsonPropertyName("WHqty")]
        public int WithheldUpdateQuantity { get; set; }

        [JsonPropertyName("Colqty")]
        public int CollateralQuantity { get; set; }

        [JsonPropertyName("CUqty")]
        public int CollateralUpdateQuantity { get; set; }

        [JsonPropertyName("WCqty")]
        public int WithheldCollateralUpdateQuantity { get; set; }

        [JsonPropertyName("Price")]
        public int AveragePrice { get; set; }

        [JsonPropertyName("Haircut")]
        public int Haircut { get; set; }

        [JsonPropertyName("LTnse")]
        public int LastTradedNSEPrice { get; set; }

        [JsonPropertyName("LTbse")]
        public int LastTradedBSEPrice { get; set; }

        [JsonPropertyName("LTmcxsxcm")]
        public int LastTradedMCXSXCMPrice { get; set; }

        [JsonPropertyName("LTcse")]
        public int LastTradedCSEPrice { get; set; }

        [JsonPropertyName("LTysx")]
        public int LastTradedYSXPrice { get; set; }

        /// <summary>
        /// Target market is the end consumer to which the company wants to sell its end products too. 
        /// Target marketing involves breaking down the entire market into various segments 
        /// and planning marketing strategies accordingly for each segment to increase the market share.
        /// </summary>
        [JsonPropertyName("Tprod")]
        public string TargetMarket { get; set; }

        [JsonPropertyName("Token1")]
        public string Token1 { get; set; }

        [JsonPropertyName("Token2")]
        public string Token2 { get; set; }

        [JsonPropertyName("Token3")]
        public string Token3 { get; set; }

        [JsonPropertyName("Token4")]
        public string Token4 { get; set; }

        [JsonPropertyName("Token5")]
        public string Token5 { get; set; }

        [JsonPropertyName("csflag")]
        public string CollateralSquareOffFlag { get; set; }

        [JsonPropertyName("hsflag")]
        public string HoldingSquareOffFlag { get; set; }

        [JsonPropertyName("SellableQty")]
        public string SellableQuantity { get; set; }

        [JsonPropertyName("ExchSeg1")]
        public string ExchangeSegment1 { get; set; }

        [JsonPropertyName("ExchSeg2")]
        public string ExchangeSegment2 { get; set; }

        [JsonPropertyName("ExchSeg3")]
        public string ExchangeSegment3 { get; set; }

        [JsonPropertyName("ExchSeg4")]
        public string ExchangeSegment4 { get; set; }

        [JsonPropertyName("ExchSeg5")]
        public string ExchangeSegment5 { get; set; }

        [JsonPropertyName("NSEHOldingValue")]
        public int NSEHoldingValue { get; set; }

        [JsonPropertyName("BSEHOldingValue")]
        public int BSEHoldingValue { get; set; }

        [JsonPropertyName("MCXHOldingValue")]
        public int MCXHoldingValue { get; set; }

        [JsonPropertyName("CSEHOldingValue")]
        public int CSEHoldingValue { get; set; }

        [JsonPropertyName("YSXHOldingValue")]
        public int YSXHoldingValue { get; set; }

        [JsonPropertyName("BuyQty")]
        public int BuyQuantity { get; set; }

        [JsonPropertyName("Series1")]
        public string Series1 { get; set; }

        [JsonPropertyName("isin")]
        public string ISIN { get; set; }

        [JsonPropertyName("authQty")]
        public string AuthorizedQuantity { get; set; }
    }
}
