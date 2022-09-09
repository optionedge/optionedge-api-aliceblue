using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Records
{
    public class SquareOffPositionParams
    {
        private string _exchangeSegment;
        [JsonProperty("exchSeg")]
        public string ExchangeSegment
        {
            get { return _exchangeSegment; }
        }

        [JsonIgnore]
        public string Exchange
        {
            get
            {
                return Constants.ExchangeToSegmentMap.Where(x => x.Value == _exchangeSegment).FirstOrDefault().Key;
            }
            set
            {
                _exchangeSegment = Constants.ExchangeToSegmentMap.ContainsKey(value) ? Constants.ExchangeToSegmentMap[value] : "";
            }
        }

        [JsonProperty("pCode")]
        public string  ProductCode;

        /// <summary>
        /// -/+ for reverse position 
        /// </summary>
        [JsonProperty("netQty")]
        public int NetQty;

        [JsonProperty("tockenNo")]
        public int InstrumentToken;

        [JsonProperty("symbol")]
        public string TradingSymbol;

    }
}
