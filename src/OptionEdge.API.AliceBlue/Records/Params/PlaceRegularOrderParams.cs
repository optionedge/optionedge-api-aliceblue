using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class PlaceRegularOrderParams
    {
        /// <summary>
        /// Optional - If not set, will be set automatically based on the order type eg Regular, Cover Order, Brac
        /// </summary>
        [JsonProperty("complexty")]
        public string Complexty;

        [JsonProperty("discqty")]
        public int DisclosedQuantity;

        [JsonProperty("exch")]
        public string Exchange;

        [JsonProperty("pCode")]
        public string ProductCode;

        [JsonProperty("prctyp")]
        public string PriceType;

        [JsonProperty("price")]
        public decimal Price;

        [JsonProperty("qty")]
        public int? Quantity;

        /// <summary>
        /// Default to Day
        /// </summary>
        [JsonProperty("ret")]
        public string RetentionType;

        [JsonProperty("symbol_id")]
        public int? InstrumentToken;

        [JsonProperty("trading_symbol")]
        public string TradingSymbol;

        [JsonProperty("transtype")]
        public string TransactionType;

        [JsonProperty("trigPrice")]
        public decimal TriggerPrice;

        [JsonProperty("orderTag")]
        public string OrderTag;
    }
}
