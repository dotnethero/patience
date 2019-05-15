using System;
using System.Collections.Generic;

namespace Patience.Core
{
    internal static class Utils
    {
        internal static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key, Func<TKey, TValue> ctor)
        {
            if (!map.ContainsKey(key))
            {
                map[key] = ctor(key);
            }
            return map[key];
        }

        internal static string[] GetLines(this string text)
        {
            return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }
    }
}
