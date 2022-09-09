using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class ExitBracketOrderParams
    {
        [JsonProperty("nestOrderNumber")]
        public string OrderNumber;

        [JsonProperty("symbolOrderId")]
        public string PriceType;

        // Why while existing, we need status
        [JsonProperty("status")]
        public string OrderStatus;
    }
}
