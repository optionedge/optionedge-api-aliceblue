using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class ScriptQuoteParams
    {
        [JsonProperty("exch")]
        public string Exchange;

        [JsonProperty("symbol")]
        public int InstrumentToken;
    }
}
