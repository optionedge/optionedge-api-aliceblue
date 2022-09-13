using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HistoryCandle
    {
        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }

        public decimal IV { get; set; }

        public long TimeData { get; set; }

        public DateTime Timestamp
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(TimeData).ToLocalTime().DateTime;
            }
        }
    }
}
