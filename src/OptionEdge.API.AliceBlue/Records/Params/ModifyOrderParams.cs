using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class ModifyOrderParams
    {
        [JsonProperty("discqty")]
        public int DisclosedQty;

        [JsonProperty("exch")]
        public string Exchange;

        [JsonProperty("filledQuantity")]
        public int FilledQty;

        [JsonProperty("nestOrderNumber")]
        public string OrderNumber;

        [JsonProperty("prctyp")]
        public string PriceType;

        [JsonProperty("price")]
        public decimal Price;

        [JsonProperty("qty")]
        public int Qty;

        [JsonProperty("trading_symbol")]
        public string TradingSymbol;

        [JsonProperty("trigPrice")]
        public decimal TriggerPrice;

        [JsonProperty("transtype")]
        public string TransactionType;

        [JsonProperty("pCode")]
        public string ProductCode;
    }
}
