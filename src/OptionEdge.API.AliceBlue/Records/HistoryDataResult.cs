using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HistoryDataResult
    {
        [JsonPropertyName("s")]
        public string Status { get; set; }

        [JsonPropertyName("t")]
        public long[] Time { get; set; }

        [JsonPropertyName("o")]
        public decimal[] Open { get; set; }

        [JsonPropertyName("h")]
        public decimal[] High { get; set; }

        [JsonPropertyName("l")]
        public decimal[] Low { get; set; }

        [JsonPropertyName("c")]
        public decimal[] Close { get; set; }

        [JsonPropertyName("v")]
        public decimal[] Volume { get; set; }

        public decimal IV { get; set; }

        public List<HistoryCandle> Candles { get; set; } = new List<HistoryCandle>();
    }
}
