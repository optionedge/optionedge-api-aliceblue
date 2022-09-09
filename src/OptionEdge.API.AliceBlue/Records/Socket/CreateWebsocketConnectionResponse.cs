using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OptionEdge.API.AliceBlue.Records
{
    public class CreateWebsocketConnectionResponse
    {
        [DataMember(Name = "t")]
        public string RequestType { get; set; }
        [DataMember(Name = "k")]
        public string Status { get; set; }

        [DataMember(Name = "s")]
        public string SetStatus
        {
            set
            {
                Status = value;
            }
        }
    }
}
