using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    internal class CreateSessonDetailResult : BaseResponseResult
    {
        [JsonPropertyName("userId")]
        public string UserID { get; set; }
        [JsonPropertyName("sessionID")]
        public string SessionID { get; set; }
    }
}
