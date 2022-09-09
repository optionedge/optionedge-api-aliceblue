using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class HistoryDataParams
    {
        [JsonProperty("exchange")]
        public string Exchange;

        [JsonProperty("token")]
        public int InstrumentToken;

        [JsonProperty("resolution")]
        public string Resolution;

        /// <summary>
        /// Unix Timestamp (milliseconds)
        /// </summary>
        [JsonProperty("from")]
        public long From;

        /// <summary>
        /// Unix Timestamp (milliseconds)
        /// </summary>
        [JsonProperty("to")]
        public long To;
    }
}
