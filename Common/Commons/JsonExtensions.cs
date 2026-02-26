using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Commons
{
    public static class JsonExtensions
    {
        private readonly static JavaScriptEncoder Encoder = 
            JavaScriptEncoder.Create(UnicodeRanges.All);
        public static JsonSerializerOptions CreateJsonSerilizerOptions(bool 
            camelCase = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { 
                Encoder = Encoder };
            if (camelCase)
            {
                options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }
            options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
            return options;
        }

        public static string ToJsonString(this object value, bool camelCase = false)
        {
            JsonSerializerOptions opt = CreateJsonSerilizerOptions(camelCase);
            return JsonSerializer.Serialize(value, opt);
        }

        public static T? ParseJson<T>(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T);
            }
            var opt = CreateJsonSerilizerOptions();
            return JsonSerializer.Deserialize<T>(value, opt);
        }
    }
}
