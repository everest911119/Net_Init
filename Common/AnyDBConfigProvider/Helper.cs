using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyDBConfigProvider
{
    public static class Helper
    {
        public static IDictionary<string, string> Clone(this IDictionary<string, string> dict)
        {
            IDictionary<string, string> cloneDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in dict)
            {
                cloneDict[kv.Key] = kv.Value;
            }
            return cloneDict;
        }


        public static bool IsChanged(this IDictionary<string, string> oldDict, IDictionary<string, string> newDic)
        {
            if (oldDict.Count != newDic.Count) return true;
            foreach (var oldKv  in oldDict)
            {
                var oldKey = oldKv.Key;
                var oldValue = oldKv.Value; 
                if (!newDic.ContainsKey(oldKey))
                {
                    return true;
                }
                var newValue = newDic[oldKey];
                if (oldValue != newValue)
                {
                    return true;
                }
            }
            return false;

        }
    }
}
