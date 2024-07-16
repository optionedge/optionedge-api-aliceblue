using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using Utf8Json;

namespace OptionEdge.API.AliceBlue.Records
{
    public class Tick
    {
        public Tick()
        {

        }
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

        public TICK_TYPE TickType { get; set; }

        private string _responeType;
        [JsonPropertyName("t")]
        [JsonProperty(PropertyName = "t")]
        [DataMember(Name = "t")]
        public string ResponseType
        {
            get
            {
                return _responeType;
            }
            set
            {
                _responeType = value;
                if (_responeType == Constants.SOCKET_RESPONSE_TYPE_TICK_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Ack;
                else if (_responeType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH_ACKNOWLEDGEMENT)
                    TickType = TICK_TYPE.Tick_Depth_Ack;
                else if (_responeType == Constants.SOCKET_RESPONSE_TYPE_TICK)
                    TickType = TICK_TYPE.Tick;
                else if (_responeType == Constants.SOCKET_RESPONSE_TYPE_TICK_DEPTH)
                    TickType = TICK_TYPE.Tick_Depth;
            }
        }

        [JsonPropertyName("e")]
        [JsonProperty(PropertyName = "e")]
        [DataMember(Name = "e")]
        public string Exchange { get; set; }

        [JsonPropertyName("tk")]
        [JsonProperty(PropertyName = "tk")]
        [DataMember(Name = "tk")]
        public string Token { get; set; }

        [JsonPropertyName("ts")]
        [JsonProperty(PropertyName = "ts")]
        [DataMember(Name = "ts")]
        public string TradingSymbol { get; set; }


        [JsonPropertyName("lp")]
        [JsonProperty(PropertyName = "lp")]
        [DataMember(Name = "lp")]
        public decimal LastTradedPrice { get; set; }

        [JsonPropertyName("pc")]
        [JsonProperty(PropertyName = "pc")]
        [DataMember(Name = "pc")]
        public decimal PercentageChange { get; set; }


        [JsonPropertyName("v")]
        [JsonProperty(PropertyName = "v")]
        [DataMember(Name = "v")]
        public string Volume { get; set; }

        [JsonPropertyName("o")]
        [JsonProperty(PropertyName = "o")]
        [DataMember(Name = "o")]
        public decimal Open { get; set; }


        [JsonPropertyName("h")]
        [JsonProperty(PropertyName = "h")]
        [DataMember(Name = "h")]
        public decimal High { get; set; }


        [JsonPropertyName("l")]
        [JsonProperty(PropertyName = "l")]
        [DataMember(Name = "l")]
        public decimal Low { get; set; }


        [JsonPropertyName("c")]
        [JsonProperty(PropertyName = "c")]
        [DataMember(Name = "c")]
        public decimal Close { get; set; }


        [JsonPropertyName("ap")]
        [JsonProperty(PropertyName = "ap")]
        [DataMember(Name = "ap")]
        public decimal AveragePrice { get; set; }


        [JsonPropertyName("bp1")]
        [JsonProperty(PropertyName = "bp1")]
        [DataMember(Name = "bp1")]
        public decimal BuyPrice1 { get; set; }


        [JsonPropertyName("sp1")]
        [JsonProperty(PropertyName = "sp1")]
        [DataMember(Name = "sp1")]
        public decimal SellPrice1 { get; set; }
    }
}

