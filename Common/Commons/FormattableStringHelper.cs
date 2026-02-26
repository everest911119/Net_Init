using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class FormattableStringHelper
    {
        public static string BuildUrl(FormattableString urlFormat)
        {
            var parameters = urlFormat.GetArguments().Select(a => FormattableString.Invariant($"{a}"));
            object[] escapedParamter = parameters.Select(s=>(object)Uri.EscapeDataString(s)).ToArray(); 
            return string.Format(urlFormat.Format, escapedParamter);
        }
        
    }
}
