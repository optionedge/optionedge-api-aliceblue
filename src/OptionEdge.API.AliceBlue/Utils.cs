using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OptionEdge.API.AliceBlue
{
    internal static class Utils
    {
        public static string GetSHA256(string data)
        {
            Encoding enc = Encoding.UTF8;
            var hashBuilder = new StringBuilder();
            var hash = SHA256.Create();
            byte[] result = hash.ComputeHash(enc.GetBytes(data));
            foreach (var b in result)
                hashBuilder.Append(b.ToString("x2"));
            return hashBuilder.ToString();
        }

        public static int ParseToInt(string value)
        {
            int result = 0;

            Int32.TryParse(value, out result);

            return result;
        }

        public static decimal ParseToDouble(string value)
        {
            decimal result = 0m;

            Decimal.TryParse(value, out result);

            return result;
        }

        public static DateTime? ToDateTimeFromUnixTimeSeconds(long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).ToLocalTime().DateTime;
        }
            
        public static DateTime? ToDateTimeFromUnixTimeSeconds(string unixTimeStamp)
        {
            long result = 0;
            long.TryParse(unixTimeStamp, out result);

            if (result == 0) return default(DateTime);

            return DateTimeOffset.FromUnixTimeSeconds(result).ToLocalTime().DateTime;
        }

        public static bool IsPropertyExist(dynamic dynamicObj, string property)
        {
            try
            {
                var value = dynamicObj[property];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LogMessage(string message, object logObject = null)
        {
            string objectJson = null;

            if (logObject != null)
            {
                objectJson = JsonConvert.SerializeObject(logObject, Formatting.Indented);
            }

            Console.WriteLine($"{DateTime.Now.ToString("G")} {message} {Environment.NewLine} {objectJson}");
        }

        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented );
        }
    }
}
