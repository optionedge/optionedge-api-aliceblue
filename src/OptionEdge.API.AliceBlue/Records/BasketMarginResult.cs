using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class BasketMarginResult : BaseResponseResult
    {
        [JsonProperty("marginUsed")]
        public decimal MarginUsed { get; set; }

        [JsonProperty("marginUsedTrade")]
        public decimal MarginUsedTrade { get; set; }
    }
}
