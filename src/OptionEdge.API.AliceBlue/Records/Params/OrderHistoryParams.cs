using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Records
{
    public class OrderHistoryParams
    {
        [JsonProperty("nestOrderNumber")]
        public string OrderNumber;       
    }
}
