using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class GetAccessTokenResult : BaseResponseResult
    {
        [JsonPropertyName("userId")]
        public string UserID { get; set; }
        [JsonPropertyName("sessionID")]
        public string AccessToken { get; set; }
    }
}
