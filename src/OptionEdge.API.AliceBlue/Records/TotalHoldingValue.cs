using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class TotalHoldingValue
    {
        public string TotalNSEHoldingValue { get; set; }

        public string TotalBSEHoldingValue { get; set; }

        public string TotalMCXHoldingValue { get; set; }

        public string TotalCSEHoldingValue { get; set; }

        public string TotalYSXHoldingValue { get; set; }
    }
}
