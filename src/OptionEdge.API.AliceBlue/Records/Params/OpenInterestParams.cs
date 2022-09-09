using Newtonsoft.Json;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Records
{
    public class OpenInterestParams
    {
        //[JsonProperty("scripList")]
        //public string Tokens
        //{
        //    get
        //    {
        //        return OpenInterestTokens
        //            .Select(x => $"{(Constants.ExchangeToSegmentMap.ContainsKey(x.Exchange) ? Constants.ExchangeToSegmentMap[x.Exchange] : Constants.EXCHANGE_NFO)}|{x.InstrumentToken}")
        //            .Aggregate((left, right) => $"{left},{right}");
        //    }
        //}

        //[JsonIgnore]
        public OpenInterestToken[] OpenInterestTokens { get; set; }
    }
}
