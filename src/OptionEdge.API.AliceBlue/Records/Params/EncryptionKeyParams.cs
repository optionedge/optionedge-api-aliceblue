using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class EncryptionKeyParams
    {
        [JsonProperty("userId")]
        public string UserId;
    }
}
