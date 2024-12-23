using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class OrderHistoryResult : BaseResponseResult
    {
        [JsonPropertyName("Trsym")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("Prc")]
        public string Price { get; set; }

        [JsonPropertyName("averageprice")]
        public string AveragePrice { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Status")]
        public string OrderStatus { get; set; }

        [JsonPropertyName("Action")]
        public string TransactionType { get; set; }

        [JsonPropertyName("Ordtype")]
        public string OrderType { get; set; }

        [JsonPropertyName("PriceNumerator")]
        public string PriceNumerator { get; set; }

        [JsonPropertyName("GeneralNumerator")]
        public string GeneralNumerator { get; set; }

        [JsonPropertyName("PriceDenomenator")]
        public string PriceDenomenator { get; set; }

        [JsonPropertyName("GeneralDenomenator")]
        public string GeneralDenomenator { get; set; }

        [JsonPropertyName("bqty")]
        public int LotSize { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("nestordernumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("nestreqid")]
        public string RequestId { get; set; }

        [JsonPropertyName("symbolname")]
        public string SymbolName { get; set; }

        [JsonPropertyName("triggerprice")]
        public decimal TriggerPrice { get; set;}

        [JsonPropertyName("disclosedqty")]
        public string DisclosedQty { get; set; }

        [JsonPropertyName("exchangeorderid")]
        public string ExchangeOrdeId { get; set; }

        [JsonPropertyName("rejectionreason")]
        public string RejectionReason { get; set; }

        [JsonPropertyName("duration")]
        public string OrderDuration { get; set; }

        [JsonPropertyName("productcode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("reporttype")]
        public string ReportType { get; set; }

        [JsonPropertyName("customerfirm")]
        public string CustomerFirm { get; set; }

        [JsonPropertyName("exchangetimestamp")]
        public string ExchangeTime { get; set; }

        [JsonPropertyName("ordersource")]
        public string OrderSource { get; set; }

        [JsonPropertyName("filldateandtime")]
        public string FilledTime { get; set; }

        [JsonPropertyName("ordergenerationtype")]
        public string OrderGenerationType { get; set; }

        [JsonPropertyName("scripname")]
        public string ScripName { get; set; }

        [JsonPropertyName("legorderindicator")]
        public string LegOrderIndicator { get; set; }

        [JsonPropertyName("filledShares")]
        public string FilledQty { get; set; }
    }
}
