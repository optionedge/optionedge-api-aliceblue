using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class TradeBookResult : BaseResponseResult
    {
        [JsonPropertyName("Exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("Trsym")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("Filledqty")]
        public int FilledQty { get; set; }

        [JsonPropertyName("Averageprice")]
        public decimal AveragePrice { get; set; }

        [JsonPropertyName("Pcode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("Nstordno")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("Trantype")]
        public string TransactionType { get; set; }

        [JsonPropertyName("FillId")]
        public int FillId { get; set; }

        [JsonPropertyName("Symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("Unfillqty")]
        public int UnfilledQty { get; set; }

        [JsonPropertyName("Exchseg")]
        public string ExchangeSegment { get; set; }

        [JsonPropertyName("Custofrm")]
        public string CustomFirm { get; set; }

        [JsonPropertyName("Prctype")]
        public string PriceType { get; set; }

        [JsonPropertyName("Ordduration")]
        public string OrderDuration { get; set; }

        [JsonPropertyName("NOReqID")]
        public string OrderRequestId { get; set; }

        [JsonPropertyName("FillLeg")]
        public int FillLeg { get; set; }

        [JsonPropertyName("Filldate")]
        public string FillDate { get; set; }

        [JsonPropertyName("Filltime")]
        public string FillTime { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("ExchordID")]
        public string ExchangeOrderId { get; set; }

        [JsonPropertyName("Time")]
        public string Time { get; set; }

        [JsonPropertyName("Exchtime")]
        public string ExchangeTime { get; set; }

        [JsonPropertyName("posflag")]
        public string PositionFlag { get; set; }

        [JsonPropertyName("Minqty")]
        public int MinimumQty { get; set; }

        [JsonPropertyName("BrokerClient")]
        public string BrokerClient { get; set; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("ReportType")]
        public string ReportType { get; set; }

        [JsonPropertyName("PriceNumerator")]
        public decimal PriceNumerator { get; set; }

        [JsonPropertyName("GeneralNumerator")]
        public decimal GeneralNumerator { get; set; }

        [JsonPropertyName("user")]
        public string UserId { get; set; }

        [JsonPropertyName("PriceDenomenator")]
        public decimal PriceDenomenator { get; set; }

        [JsonPropertyName("GeneralDenomenator")]
        public decimal GeneralDenomenator { get; set; }

        [JsonPropertyName("bqty")]
        public int LotSize { get; set; }

        [JsonPropertyName("companyname")]
        public string CompanyName { get; set; }

        [JsonPropertyName("series")]
        public string Series { get; set; }

        [JsonPropertyName("ordergenerationtype")]
        public string OrderGenerationType { get; set; }

        [JsonPropertyName("remarks")]
        public string Remarks { get; set; }

        [JsonPropertyName("symbolname")]
        public string SymbolName { get; set; }

        [JsonPropertyName("iSinceBOE")]
        public int iSinceBOE { get; set; }

        [JsonPropertyName("usecs")]
        public string Usecs { get; set; }

        [JsonPropertyName("Expiry")]
        public string Expiry { get; set; }

        [JsonPropertyName("expdate")]
        public string ExpiryDate { get; set; }

        [JsonPropertyName("strikeprice")]
        public decimal Strike { get; set; }

        [JsonPropertyName("optionType")]
        public string OptionType { get; set; }

        [JsonPropertyName("AlgoID")]
        public string AlgoID { get; set; }

        [JsonPropertyName("AlgoCategory")]
        public string AlgoCategory { get; set; }

        [JsonPropertyName("panNo")]
        public string PANNo { get; set; }
    }
}
