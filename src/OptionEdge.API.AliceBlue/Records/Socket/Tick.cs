using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Utf8Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class Tick
    {
        public Tick(dynamic data)
        {
            try
            {
                /// IMP:
                /// Inefficient handling of dynamic missing property check
                /// To be updated in coming versions
                /// https://stackoverflow.com/questions/2839598/how-to-detect-if-a-property-exists-on-an-expandoobject

                var responsType = data["t"];

                Exchange = data["e"];
                Token = Utils.ParseToInt(data["tk"]);

                if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Ack;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Depth_Ack;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK)
                    TickType = TICK_TYPE.Tick;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH)
                    TickType = TICK_TYPE.Tick_Depth;

                TradingSymbol = Utils.IsPropertyExist(data, "ts") ? data["ts"] : null;

                Close = Utils.IsPropertyExist(data, "c") ? Utils.ParseToDouble(data["c"]) : 0;
                LastTradedPrice = Utils.IsPropertyExist(data, "lp") ? Utils.ParseToDouble(data["lp"]) : 0;
                PercentageChange = Utils.IsPropertyExist(data, "pc") ? Utils.ParseToDouble(data["pc"]) : 0;

                PreviousDayClose = Close.Value;

                Open = Utils.IsPropertyExist(data, "o") ? Utils.ParseToDouble(data["o"]) : 0;
                High = Utils.IsPropertyExist(data, "h") ? Utils.ParseToDouble(data["h"]) : 0;
                Low = Utils.IsPropertyExist(data, "l") ? Utils.ParseToDouble(data["l"]) : 0;

                AveragePrice = Utils.IsPropertyExist(data, "ap") ? Utils.ParseToDouble(data["ap"]) : 0;
                Volume = Utils.IsPropertyExist(data, "v") ? Utils.ParseToInt(data["v"]) : 0;

                BuyPrice1 = Utils.IsPropertyExist(data, "bp1") ? Utils.ParseToDouble(data["bp1"]) : 0;
                SellPrice1 = Utils.IsPropertyExist(data, "sp1") ? Utils.ParseToDouble(data["sp1"]) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public TICK_TYPE TickType { get; }
        public string Exchange { get; }

        public int? Token { get; }

        public string TradingSymbol { get; }
        public decimal? LastTradedPrice { get; set; }

        public decimal? PercentageChange { get; set; }

        public decimal PreviousDayClose { get; set; }

        [JsonFormatter(typeof(decimal))]
        public decimal? ChangeValue { get; set; }

        public int? Volume { get; set; }

        public decimal? Open { get; set; }

        public decimal? High { get; set; }

        public decimal? Low { get; set; }

        public decimal? Close { get; set; }

        public decimal? AveragePrice { get; set; }

        public decimal BuyPrice1 { get; set; }

        public decimal SellPrice1 { get; set; }
    }
}

