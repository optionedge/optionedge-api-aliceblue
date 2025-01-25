using Newtonsoft.Json;
using System.Linq;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue
{
    public class HistoryDataParams
    {
        [JsonPropertyName("exchange")]
        public string Exchange;

        [JsonPropertyName("token")]
        public int InstrumentToken;

        /// <summary>
        /// "1", "2", "3", "4", "5", "10", "15", "30", "60", "120", "180", "240", "D", "1W", "1M"
        /// </summary>
        [JsonPropertyName("resolution")]
        public string Interval;

        /// <summary>
        /// Unix Timestamp (seconds)
        /// </summary>
        [JsonPropertyName("from")]
        public long From;

        /// <summary>
        /// Unix Timestamp (seconds)
        /// </summary>
        [JsonPropertyName("to")]
        public long To;

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("countback")]
        public int Countback { get; set; }
    }
}
