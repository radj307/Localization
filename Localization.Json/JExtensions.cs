using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Localization.Json
{
    internal static class JExtensions
    {
        public static bool TryFindChild<T>(this JObject parent, Func<string, T, bool> predicate, out T child, out string childKey) where T : JToken
        {
            foreach (var (key, value) in parent)
            {
                if (value is T valueWithType)
                {
                    if (predicate(key, valueWithType))
                    {
                        childKey = key;
                        child = valueWithType;
                        return true;
                    }
                }
            }
            child = null!;
            childKey = null!;
            return false;
        }
        public static bool TryFindChild<T>(this JObject parent, Func<string, T, bool> predicate, out T child) where T : JToken
            => TryFindChild(parent, predicate, out child, out _);
        public static bool TryFindChild(this JObject parent, Func<string, JToken, bool> predicate, out JToken child, out string childKey)
            => TryFindChild<JToken>(parent, predicate, out child, out childKey);
        public static bool TryFindChild(this JObject parent, Func<string, JToken, bool> predicate, out JToken child)
            => TryFindChild(parent, predicate, out child, out _);
    }
}
