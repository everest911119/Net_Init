using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string s1,string s2)
        {
            return string.Equals(s1,s2,StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// 截取字符串s1最多前maxLen个字符
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="maxLen"></param>
        /// <returns></returns>
        public static string Cut(this string s1, int maxLen)
        {
            if (s1 == null)
            {
                return string.Empty;
            }
            int len = s1.Length<= maxLen ? s1.Length : maxLen;
            return s1.Substring(0, len);
        }
    }
}
