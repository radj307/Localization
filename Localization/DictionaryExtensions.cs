using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization
{
    public static class DictionaryExtensions
    {
        public static bool TryGetValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string key, StringComparison stringComparison, out TValue value)
        {
            foreach (var (k, v) in dictionary)
            {
                if (k.Equals(key, stringComparison))
                {
                    value = v;
                    return true;
                }
            }
            value = default!;
            return false;
        }
        public static IReadOnlyDictionary<TKey, IReadOnlyDictionary<TSubKey, TValue>> AsReadOnlyDictionary<TKey, TSubKey, TValue>(this Dictionary<TKey, Dictionary<TSubKey, TValue>> dictionary)
        {
            return (IReadOnlyDictionary<TKey, IReadOnlyDictionary<TSubKey, TValue>>)dictionary.Select(d => (d.Key, (IReadOnlyDictionary<string, string>)d.Value)).ToDictionary(pr => pr.Key, pr => pr.Item2);
        }
    }
}
