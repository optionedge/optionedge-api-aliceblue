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
                FeedTime = Utils.IsPropertyExist(data, "ft") ? Utils.ToDateTimeFromUnixTimeSeconds(data["ft"]) : null;

                if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Ack;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Depth_Ack;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK)
                    TickType = TICK_TYPE.Tick;
                else if (responsType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH)
                    TickType = TICK_TYPE.Tick_Depth;

                TradingSymbol = Utils.IsPropertyExist(data, "ts") ? data["ts"] : null;

                LotSize = Utils.IsPropertyExist(data, "ls") ? Utils.ParseToInt(data["ls"]) : 0;
                TickSize = Utils.IsPropertyExist(data, "ti") ? Utils.ParseToDouble(data["ti"]) : 0;
                Close = Utils.IsPropertyExist(data, "c") ? Utils.ParseToDouble(data["c"]) : 0;
                LastTradedPrice = Utils.IsPropertyExist(data, "lp") ? Utils.ParseToDouble(data["lp"]) : 0;
                PercentageChange = Utils.IsPropertyExist(data, "pc") ? Utils.ParseToDouble(data["pc"]) : 0;
                Open = Utils.IsPropertyExist(data, "o") ? Utils.ParseToDouble(data["o"]) : 0;
                High = Utils.IsPropertyExist(data, "h") ? Utils.ParseToDouble(data["h"]) : 0;
                Low = Utils.IsPropertyExist(data, "l") ? Utils.ParseToDouble(data["l"]) : 0;

                AveragePrice = Utils.IsPropertyExist(data, "ap") ? Utils.ParseToDouble(data["ap"]) : 0;
                Volume = Utils.IsPropertyExist(data, "v") ? Utils.ParseToInt(data["v"]) : 0;

                BuyPrice1 = Utils.IsPropertyExist(data, "bp1") ? Utils.ParseToDouble(data["bp1"]) : 0;
                SellPrice1 = Utils.IsPropertyExist(data, "sp1") ? Utils.ParseToDouble(data["sp1"]) : 0;
                BuyQty1 = Utils.IsPropertyExist(data, "bq1") ? Utils.ParseToInt(data["bq1"]) : 0;
                SellQty1 = Utils.IsPropertyExist(data, "sq1") ? Utils.ParseToInt(data["sq1"]) : 0;

                TotalOpenInterest = Utils.IsPropertyExist(data, "toi") ? Utils.ParseToInt(data["toi"]) : 0;

                LastTradedQty = Utils.IsPropertyExist(data, "ltq") ? Utils.ParseToInt(data["ltq"]) : 0;
                TotalBuyQty = Utils.IsPropertyExist(data, "tbq") ? Utils.ParseToInt(data["tbq"]) : 0;
                TotalSellQty = Utils.IsPropertyExist(data, "tsq") ? Utils.ParseToInt(data["tsq"]) : 0;

                BuyPrice2 = Utils.IsPropertyExist(data, "bp2") ? Utils.ParseToDouble(data["bp2"]) : 0;
                SellPrice2 = Utils.IsPropertyExist(data, "sp2") ? Utils.ParseToDouble(data["sp2"]) : 0;

                BuyPrice3 = Utils.IsPropertyExist(data, "bp3") ? Utils.ParseToDouble(data["bp3"]) : 0;
                SellPrice3 = Utils.IsPropertyExist(data, "sp3") ? Utils.ParseToDouble(data["sp3"]) : 0;

                BuyPrice4 = Utils.IsPropertyExist(data, "bp4") ? Utils.ParseToDouble(data["bp4"]) : 0;
                SellPrice4 = Utils.IsPropertyExist(data, "sp4") ? Utils.ParseToDouble(data["sp4"]) : 0;

                BuyPrice5 = Utils.IsPropertyExist(data, "bp5") ? Utils.ParseToDouble(data["bp5"]) : 0;
                SellPrice5 = Utils.IsPropertyExist(data, "sp5") ? Utils.ParseToDouble(data["sp5"]) : 0;

                BuyQty2 = Utils.IsPropertyExist(data, "bq2") ? Utils.ParseToInt(data["bq2"]) : 0;
                SellQty2 = Utils.IsPropertyExist(data, "sq2") ? Utils.ParseToInt(data["sq2"]) : 0;

                BuyQty3 = Utils.IsPropertyExist(data, "bq3") ? Utils.ParseToInt(data["bq3"]) : 0;
                SellQty3 = Utils.IsPropertyExist(data, "sq3") ? Utils.ParseToInt(data["sq3"]) : 0;

                BuyQty4 = Utils.IsPropertyExist(data, "bq4") ? Utils.ParseToInt(data["bq4"]) : 0;
                SellQty4 = Utils.IsPropertyExist(data, "sq4") ? Utils.ParseToInt(data["sq4"]) : 0;

                BuyQty5 = Utils.IsPropertyExist(data, "bq5") ? Utils.ParseToInt(data["bq5"]) : 0;
                SellQty5 = Utils.IsPropertyExist(data, "sq5") ? Utils.ParseToInt(data["sq5"]) : 0;

                UpperCircuit = Utils.IsPropertyExist(data, "uc") ? Utils.ParseToDouble(data["uc"]) : 0;
                LowerCircuit = Utils.IsPropertyExist(data, "lc") ? Utils.ParseToDouble(data["lc"]) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public TICK_TYPE TickType { get; }
        public string Exchange { get;  }

        public int? Token { get;  }

        public string TradingSymbol { get; }
        public int LotSize { get; }

        public decimal TickSize { get; set; }

        public decimal? LastTradedPrice { get; set; }

        public DateTime? FeedTime { get; set; }

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

        public decimal OpenInterest { get; set; }

        public decimal BuyPrice1 { get; set; }

        public decimal BuyQty1 { get; set; }

        public decimal SellPrice1 { get; set; }

        public decimal SellQty1 { get; set; }

        public int LastTradedQty { get; set; }

        public int TotalBuyQty { get; set; }
        public int TotalSellQty { get; set; }
        public int TotalOpenInterest { get; set; }


        public decimal BuyPrice2 { get; set; }

        public decimal BuyQty2 { get; set; }

        public decimal SellPrice2 { get; set; }

        public decimal SellQty2 { get; set; }



        public decimal BuyPrice3 { get; set; }

        public decimal BuyQty3 { get; set; }

        public decimal SellPrice3 { get; set; }

        public decimal SellQty3 { get; set; }



        public decimal BuyPrice4 { get; set; }

        public decimal BuyQty4 { get; set; }

        public decimal SellPrice4 { get; set; }

        public decimal SellQty4 { get; set; }



        public decimal BuyPrice5 { get; set; }

        public decimal BuyQty5 { get; set; }

        public decimal SellPrice5 { get; set; }

        public decimal SellQty5 { get; set; }

        public decimal UpperCircuit { get; set; }
        public decimal LowerCircuit { get; set; }
    }
}
