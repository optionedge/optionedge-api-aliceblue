using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    internal class WebsocketSessionResult : BaseResponseResult
    {
        [JsonPropertyName("result")]
        public WebsocketSession Result { get; set; }
    }
}
