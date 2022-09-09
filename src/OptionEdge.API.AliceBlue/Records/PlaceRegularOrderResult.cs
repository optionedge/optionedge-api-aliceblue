using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class PlaceRegularOrderResult : BaseResponseResult
    {
        [JsonPropertyName("NOrdNo")]
        public string OrderNumber { get; set; }       
    }
}
