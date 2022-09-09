using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class AccountDetails : BaseResponseResult
    {
        [JsonPropertyName("accountStatus")]
        public string AccountStatus { get; set; }
        [JsonPropertyName("dpType")]
        public string DPType { get; set; }
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }
        [JsonPropertyName("sBrokerName")]
        public string BrokerName { get; set; }
        [JsonPropertyName("product")]
        public string[] Product { get; set; }
        [JsonPropertyName("accountName")]
        public string AccountName { get; set; }
        [JsonPropertyName("cellAddr")]
        public string Mobile { get; set; }
        [JsonPropertyName("emailAddr")]
        public string Email { get; set; }
        [JsonPropertyName("exchEnabled")]
        public string ExchangeEnabled { get; set; }
    }
}
