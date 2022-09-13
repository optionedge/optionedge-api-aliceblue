using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue
{
    public class HistoryDataParams
    {
        public string Exchange;

        public int InstrumentToken;

        /// <summary>
        /// "1", "2", "3", "4", "5", "10", "15", "30", "60", "120", "180", "240", "D", "1W", "1M"
        /// </summary>
        public string Interval;

        /// <summary>
        /// Unix Timestamp (seconds)
        /// </summary>
        public long From;

        /// <summary>
        /// Unix Timestamp (seconds)
        /// </summary>
        public long To;

        public bool Index { get; set; }
    }
}
