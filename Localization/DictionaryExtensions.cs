using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization
{
    /// <summary>
    /// Extension methods for dictionary types.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the <paramref name="value"/> associated with the specified <paramref name="key"/>. Comparisons use the specified <paramref name="stringComparison"/> type.
        /// </summary>
        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
        /// <param name="dictionary"/><param name="key"/><param name="value"/>
        /// <param name="stringComparison">The string comparison type to use when comparing keys.</param>
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
        /// <summary>
        /// Gets the value of the first available key.
        /// </summary>
        /// <param name="dictionary">The dictionary instance to search.</param>
        /// <param name="keys">Any number of keys to search for, in order of priority.</param>
        /// <param name="stringComparison">The string comparison type to use when comparing keys.</param>
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
            => dictionary.TryGetFirstValue(keys, stringComparison, out _, out value);
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
        /// <summary>
        /// Gets nested dictionaries as nested <see cref="IReadOnlyDictionary{TKey, TValue}"/> instances.
        /// </summary>
        /// <typeparam name="TKey">The type of key in the parent dictionary.</typeparam>
        /// <typeparam name="TSubKey">The type of key in the nested dictionaries.</typeparam>
        /// <typeparam name="TSubValue">The type of value in the nested dictionaries.</typeparam>
        public static IReadOnlyDictionary<TKey, IReadOnlyDictionary<TSubKey, TSubValue>> ToReadOnlyDictionary<TKey, TSubKey, TSubValue>(this IReadOnlyDictionary<TKey, Dictionary<TSubKey, TSubValue>> dictionary)
        {
            return dictionary.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<TSubKey, TSubValue>)kvp.Value);
        }
        /// <summary>
        /// Gets the value of the specified <paramref name="key"/>, or creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of key in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of value in the dictionary. It must be default-constructible to use this overload; otherwise,<br/>use <see cref="GetOrCreateValue{TKey, TValue}(IDictionary{TKey, TValue}, TKey, Func{TValue})"/>.</typeparam>
        /// <param name="dictionary">The dictionary instance.</param>
        /// <param name="key">The key of the element to get/add.</param>
        /// <returns>The value of the element.</returns>
        public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var existingValue))
                return existingValue;
            else
            {
                var value = new TValue();
                dictionary.Add(key, value);
                return value;
            }
        }
        /// <summary>
        /// Gets the value of the specified <paramref name="key"/>, or creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of key in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of value in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary instance.</param>
        /// <param name="key">The key of the element to get/add.</param>
        /// <param name="valueFactory">A function that returns a new instance of type <typeparamref name="TValue"/>.</param>
        /// <returns>The value of the element.</returns>
        public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var existingValue))
                return existingValue;
            else
            {
                var value = valueFactory();
                dictionary.Add(key, value);
                return value;
            }
        }
    }
}
