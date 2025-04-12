using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class BasketMarginItem
    {
        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("tradingSymbol")]
        public string TradingSymbol { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        public int Quantity { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("priceType")]
        public string PriceType { get; set; }

        [JsonProperty("token")]
        public int Token { get; set; }

        [JsonProperty("transType")]
        public string TransactionType { get; set; }
    }
}
