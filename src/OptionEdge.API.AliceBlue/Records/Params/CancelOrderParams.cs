using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class CancelOrderParams
    {
        [JsonProperty("nestOrderNumber")]
        public string OrderNumber;
    }
}
