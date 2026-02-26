using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnyDBConfigProvider
{
    public static class JsonelementExtension
    {
        public static string? GetValueForConfig(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }else if (element.ValueKind == JsonValueKind.Null || element.ValueKind== JsonValueKind.Undefined)
            {
                return null;
            }else
            {
                return element.GetRawText();
            }
        }
    }
}
