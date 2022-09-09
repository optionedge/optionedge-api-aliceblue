using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class UnsubscribeMarketDataRequest
    {
        [DataMember(Name = "t")]
        public string RequestType { get; set; } = "u";

        [DataMember(Name = "k")]
        public string Tokens { get
            {
                return SubscribedTokens
                    .Select(x => $"{x.Exchange}|{x.Token}")
                    .Aggregate((left, right) => $"{left}#{right}");
            }
        }

        [IgnoreDataMember]
        public SubscriptionToken[] SubscribedTokens { get; set; }

    }
}
