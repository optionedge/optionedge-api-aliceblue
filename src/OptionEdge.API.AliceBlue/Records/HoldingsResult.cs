using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HoldingsResult : BaseResponseResult
    {
        [JsonPropertyName("clientid")]
        public string Clientid { get; set; }

        [JsonPropertyName("HoldingVal")]
        public HoldingValue[] HoldingValue { get; set; }

        [JsonPropertyName("Totalval")]
        public HoldingValue[] TotalValue { get; set; }

    }
}
