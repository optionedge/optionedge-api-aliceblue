using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class WebsocketAccessTokenParams
    {
        [JsonProperty("loginType")]
        public string LoginType;
    }
}
