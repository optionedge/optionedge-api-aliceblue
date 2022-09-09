using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionEdge.API.AliceBlue.Samples
{
    public static class Utils
    {
        public static void LogMessage(string message, object logObject = null)
        {
            string objectJson = null;

            if (logObject != null)
            {
                objectJson = Serialize(logObject);
            }

            Console.WriteLine($"{DateTime.Now.ToString("G")} {message} {Environment.NewLine} {objectJson}");
        }

        public static string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
