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
            var newLine = text.IndexOf("\r", StringComparison.Ordinal) > -1 ? "\r\n" : "\n";
            return text.Split(new[] { newLine }, StringSplitOptions.None);
        }
    }
}
