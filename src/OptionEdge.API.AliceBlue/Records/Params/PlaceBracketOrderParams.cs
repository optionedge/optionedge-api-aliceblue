using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class PlaceBracketOrderParams : PlaceCoverOrderParams
    {
        [JsonProperty("target")]
        public decimal Target;
    }
}
