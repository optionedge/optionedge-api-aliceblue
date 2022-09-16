using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue
{
    public class Constants
    {
        public const string STATUS_OK = "Ok";
        public const string STATUS_NOT_OK = "Not_Ok";

        public const string ORDER_STATUS_OPEN = "OPEN";
        public const string ORDER_STATUS_PENDING = "PENDING";

        public const string TRANSACTION_TYPE_BUY = "BUY";
        public const string TRANSACTION_TYPE_SELL = "SELL";

        public const string POSITION_NETWISE = "NET";
        public const string POSITION_DAYWISE = "DAY";

        public const string RETENTION_TYPE_DAY = "DAY";
        public const string RETENTION_TYPE_IOC = "IOC";

        public const string PRICE_TYPE_LIMIT = "L";
        public const string PRICE_TYPE_MARKET = "MKT";
        public const string PRICE_TYPE_SL_LIMIT = "SL";
        public const string PRICE_TYPE_SL_MARKET = "SL-M";

        public const string PRODUCT_CODE_MIS = "MIS";
        public const string PRODUCT_CODE_NRML = "NRML";
        public const string PRODUCT_CODE_CNC = "CNC";
        public const string PRODUCT_CODE_CO = "CO";
        public const string PRODUCT_CODE_BO = "BO";

        public const string SEGMENT_TYPE_CASH = "CASH";
        public const string SEGMENT_TYPE_FO = "FO";
        public const string SEGMENT_TYPE_COMMODITY = "COM";
        public const string SEGMENT_TYPE_CURRENCY = "CUR";
        public const string SEGMENT_TYPE_ALL = "ALL";

        public const string HISTORICAL_DATA_RESOLUTION_1_MINUTE = "1";
        public const string HISTORICAL_DATA_RESOLUTION_2_MINUTE = "2";
        public const string HISTORICAL_DATA_RESOLUTION_3_MINUTE = "3";
        public const string HISTORICAL_DATA_RESOLUTION_5_MINUTE = "5";
        public const string HISTORICAL_DATA_RESOLUTION_10_MINUTE = "10";
        public const string HISTORICAL_DATA_RESOLUTION_15_MINUTE = "15";
        public const string HISTORICAL_DATA_RESOLUTION_30_MINUTE = "30";
        public const string HISTORICAL_DATA_RESOLUTION_1_HOURS = "60";
        public const string HISTORICAL_DATA_RESOLUTION_2_HOUR = "120";
        public const string HISTORICAL_DATA_RESOLUTION_3_HOUR = "180";
        public const string HISTORICAL_DATA_RESOLUTION_4_HOUR = "240";
        public const string HISTORICAL_DATA_RESOLUTION_1_DAY = "D";
        public const string HISTORICAL_DATA_RESOLUTION_1_WEEK = "1W";
        public const string HISTORICAL_DATA_RESOLUTION_1_MONTH = "1M";


        internal const string EXCHANGE_SEGMENT_NSE_CM = "nse_cm";
        internal const string EXCHANGE_SEGMENT_BSE_CM = "base_cm";
        internal const string EXCHANGE_SEGMENT_NSE_FO = "nse_fo";
        internal const string EXCHANGE_SEGMENT_MCX_FO = "mcx_fo";
        internal const string EXCHANGE_SEGMENT_CDS_CDE_FO = "cde_fo";
        internal const string EXCHANGE_SEGMENT_BFO_MCX_SX = "mcx_sx";
        internal const string EXCHANGE_SEGMENT_BCD_BCS_FO = "bcs_fo";
        internal const string EXCHANGE_SEGMENT_NCO_NSE_COM = "nse_com";
        internal const string EXCHANGE_SEGMENT_BCO_BSE_COM = "bse_com";

        public const string EXCHANGE_NSE = "NSE";
        public const string EXCHANGE_NFO = "NFO";
        public const string EXCHANGE_CDS = "CDS";
        public const string EXCHANGE_BSE = "BSE";
        public const string EXCHANGE_BFO = "BFO";
        public const string EXCHANGE_BCD = "BCD";
        public const string EXCHANGE_MCX = "MCX";
        public const string EXCHANGE_NCO = "NCO";
        public const string EXCHANGE_BCO = "BCO";
        public const string EXCHANGE_INDICES = "INDICES";

        public const string ORDER_HISTORY_COMPLETE = "complete";
        public const string ORDER_HISTORY_REJECTED = "rejected";
        public const string ORDER_HISTORY_PENDING = "pending";

        public const string ORDER_COMPLEXITY_REGULAR = "regular";
        public const string ORDER_COMPLEXITY_BRACKET_ORDER = "bo";
        public const string ORDER_COMPLEXITY_COVER_ORDER = "co";

        public const string API_RESPONSE_STATUS_OK = "Ok";
        public const string API_RESPONSE_STATUS_Not_OK = "Not_Ok";

        public const string TICK_MODE_QUOTE = "QUOTE";
        public const string TICK_MODE_FULL = "FULL";

        public const string SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_MARKET = "t";
        public const string SUBSCRIBE_SOCKET_TICK_DATA_REQUEST_TYPE_DEPTH = "d";

        public const string SOCKET_RESPONSE_TYPE_TICK_ACKNOWLEDGEMENT = "tk";
        public const string SOCKET_RESPONSE_TYPE_TICK = "tf";
        public const string SOCKET_RESPONSE_TYPE_TICK_DEPTH_ACKNOWLEDGEMENT = "dk";
        public const string SOCKET_RESPONSE_TYPE_TICK_DEPTH = "df";

        internal static Dictionary<string, string> ExchangeToSegmentMap = new Dictionary<string, string>()
        {
            {
                Constants.EXCHANGE_NSE, Constants.EXCHANGE_SEGMENT_NSE_CM
            },
            {
                Constants.EXCHANGE_BSE, Constants.EXCHANGE_SEGMENT_BSE_CM
            },
            {
                Constants.EXCHANGE_NFO, Constants.EXCHANGE_SEGMENT_NSE_FO
            },
            {
                Constants.EXCHANGE_MCX, Constants.EXCHANGE_SEGMENT_MCX_FO
            },
            {
                Constants.EXCHANGE_CDS, Constants.EXCHANGE_SEGMENT_CDS_CDE_FO
            },
            {
                Constants.EXCHANGE_BFO, Constants.EXCHANGE_SEGMENT_BFO_MCX_SX
            },
            {
                Constants.EXCHANGE_BCD, Constants.EXCHANGE_SEGMENT_BCD_BCS_FO
            },
            {
                Constants.EXCHANGE_NCO, Constants.EXCHANGE_SEGMENT_NCO_NSE_COM
            },
            {
                Constants.EXCHANGE_BCO, Constants.EXCHANGE_SEGMENT_BCO_BSE_COM
            },
        };
    }
}
