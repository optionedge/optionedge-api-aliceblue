using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class BasketMarginResult : BaseResponseResult
    {
        [JsonProperty("result")]
        public MarginResultData Result { get; set; }
    }

    public class MarginResultData
    {
        [JsonProperty("marginUsed")]
        public decimal MarginUsed { get; set; }

        [JsonProperty("marginUsedTrade")]
        public Decimal MarginUsedTrade { get; set; }
    }
}
