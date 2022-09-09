using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class ExitCoverOrderParams
    {
        [JsonProperty("nestOrderNumber")]
        public string OrderNumber;
    }
}
