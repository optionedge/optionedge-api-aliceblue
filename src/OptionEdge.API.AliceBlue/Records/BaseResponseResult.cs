using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class BaseResponseResult
    {
        [JsonPropertyName("stat")]
        [JsonProperty(PropertyName = "stat")]
        [DataMember(Name = "stat")]
        public string Status { get; set; }
        [JsonPropertyName("emsg")]
        [JsonProperty(PropertyName = "emsg")]
        [DataMember(Name = "emsg")]
        public string ErrorMessage { get; set; }
    }
}
