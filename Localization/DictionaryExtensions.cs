using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization
{
    public static class DictionaryExtensions
    {
        internal static bool TryGetValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string key, StringComparison stringComparison, out TValue value)
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
        /// <summary>
        /// Gets the value of the first available key.
        /// </summary>
        /// <param name="dictionary">The dictionary instance to search.</param>
        /// <param name="keys">Any number of keys to search for, in order of priority.</param>
        /// <param name="foundKey">The key when found; otherwise, <see langword="null"/>.</param>
        /// <param name="value">The value when found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when a key was found and <paramref name="foundKey"/>/<paramref name="value"/> are not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetFirstValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, IEnumerable<string> keys, StringComparison stringComparison, out string foundKey, out TValue value)
        {
            foreach (var key in keys)
            {
                if (dictionary.TryGetValue(key, stringComparison, out value))
                {
                    foundKey = key;
                    return true;
                }
            }
            foundKey = null!;
            value = default!;
            return false;
        }
        /// <inheritdoc cref="TryGetFirstValue{TValue}(IReadOnlyDictionary{string, TValue}, IEnumerable{string}, StringComparison, out string, out TValue)"/>
        public static bool TryGetFirstValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, IEnumerable<string> keys, StringComparison stringComparison, out TValue value)
            => TryGetFirstValue(dictionary, keys, stringComparison, out _, out value);
        /// <summary>
        /// Gets the value of the first available key.
        /// </summary>
        /// <param name="dictionary">The dictionary instance to search.</param>
        /// <param name="keys">Any number of keys to search for, in order of priority.</param>
        /// <param name="foundKey">The key when found; otherwise, <see langword="null"/>.</param>
        /// <param name="value">The value when found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when a key was found and <paramref name="foundKey"/>/<paramref name="value"/> are not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetFirstValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, IEnumerable<string> keys, out string foundKey, out TValue value)
        {
            foreach (var key in keys)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    foundKey = key;
                    return true;
                }
            }
            foundKey = null!;
            value = default!;
            return false;
        }
        /// <inheritdoc cref="TryGetFirstValue{TValue}(IReadOnlyDictionary{string, TValue}, IEnumerable{string}, out string, out TValue)"/>
        public static bool TryGetFirstValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, IEnumerable<string> keys, out TValue value)
            => dictionary.TryGetFirstValue(keys, out _, out value);
        public static IReadOnlyDictionary<TKey, IReadOnlyDictionary<TSubKey, TValue>> AsReadOnlyDictionary<TKey, TSubKey, TValue>(this Dictionary<TKey, Dictionary<TSubKey, TValue>> dictionary)
        {
            return (IReadOnlyDictionary<TKey, IReadOnlyDictionary<TSubKey, TValue>>)dictionary.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<TKey, TValue>)kvp.Value);
        }
    }
}
