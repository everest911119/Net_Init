using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class EnumberableExtension
    {
        public static bool SequenceIgnoredEqual<T>(this IEnumerable<T> item1, IEnumerable<T> item2)
        {
            if (item1 == item2)
            {
                return true;
            }else if (item2 == null|| item1== null)
            {
                return false;
            }
            return item1.OrderBy(e=>e).SequenceEqual(item2.OrderBy(e=>e));  
        }
    }
}
