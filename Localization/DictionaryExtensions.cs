using System;
using System.Collections.Generic;

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
    }
}
