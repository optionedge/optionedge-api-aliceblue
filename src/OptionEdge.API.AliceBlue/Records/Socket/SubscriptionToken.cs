using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class SubscriptionToken
    {
        public string Exchange { get; set; }
        public int Token { get; set; }

        public override int GetHashCode()
        {
            return $"{Exchange}{Token}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            SubscriptionToken other = obj as SubscriptionToken;

            return Exchange.Equals(other.Exchange) && Token == other.Token;
        }
    }
}
