using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class HistoryDataResult : BaseResponseResult
    {
        [JsonPropertyName("result")]
        public HistoryCandle[] Result { get; set; }
       
    }
}
