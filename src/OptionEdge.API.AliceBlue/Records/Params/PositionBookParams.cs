using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class PositionBookParams
    {
        /// <summary>
        /// Retention type (from LoadRetentionType rest api).These orders state the system to keep the orders pending until 
        /// the market price reaches the specified limit order price. 
        /// The various retention orders are: Day or End of Session Order: 
        /// This is the most commonly used retention type
        /// </summary>
        [JsonProperty("ret")]
        public string RetentionType;
    }
}
