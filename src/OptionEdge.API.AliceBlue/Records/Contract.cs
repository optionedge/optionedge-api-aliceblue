using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class Contract
    {
        public string Exchange { get; set; }

        public string ExchangeSegment { get; set; }

        public DateTime? Expiry { get; set; }

        public string InstrumentName { get; set; }

        public string FormattedInstrumentName { get; set; }

        public string InstrumentType { get; set; }

        public int LotSize { get; set; }

        public string OptionType { get; set; }

        public decimal Strike { get; set; }

        public string InstrumentSymbol { get; set; }

        public decimal TickSize { get; set; }

        public int InstrumentToken { get; set; }

        public string TradingSymbol { get; set; }

    }
}
