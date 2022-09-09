using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HistoryCandle
    {
        [JsonPropertyName("Open")]
        public decimal Open { get; set; }

        [JsonPropertyName("High")]
        public decimal High { get; set; }

        [JsonPropertyName("Low")]
        public decimal Low { get; set; }

        [JsonPropertyName("Close")]
        public decimal Close { get; set; }

        [JsonPropertyName("Volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("Time")]
        public string TimeData { get; set; }

        [IgnoreDataMember]
        public DateTime Timestamp
        {
            get
            {
                return DateTime.Parse(TimeData);
            }
        }
    }
}
