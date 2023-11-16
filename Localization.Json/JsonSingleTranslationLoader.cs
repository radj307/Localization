using Localization.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization.Json
{
    /// <summary>
    /// <see cref="ITranslationLoader"/> for JSON files that uses an alternative syntax that is easier to read &amp; write, but has the limitation of only supporting 1 language per file.
    /// </summary>
    /// <remarks>
    /// Syntax example:
    /// <code>
    /// {
    ///   "$LanguageName": "English",
    ///   
    ///   "MainWindow": {
    ///     "Text": "Some text"
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public class JsonSingleTranslationLoader : JsonTranslationLoader, ITranslationLoader
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified <paramref name="serializedData"/> using the single language syntax.
        /// </summary>
        /// <param name="serializedData">A string containing serialized JSON data using the single language syntax.</param>
        /// <returns>The deserialized language, or <see langword="null"/> if the syntax is invalid for this converter.</returns>
        /// <exception cref="InvalidOperationException">A JSON token of an unexpected type was found. Only Objects &amp; Strings are allowed.</exception>
        public override Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            if (JsonConvert.DeserializeObject(serializedData) is JObject root
                && root.TryFindChild<JValue>((key, node) => key.Equals("$LanguageName", StringComparison.OrdinalIgnoreCase) && node.Type == JTokenType.String, out var langNameNode, out var langNameNodeKey))
            { // this is a single-language JSON file
                root.Remove(langNameNodeKey); //< don't include "$LanguageName" in the translations
                var translations = new Dictionary<string, string>();

                var stack = new Stack<JToken>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    switch (current.Type)
                    {
                    case JTokenType.Property:
                        {
                            var property = (JProperty)current;

                            if (property.Value is JObject obj)
                            {
                                current = obj;
                                goto case JTokenType.Object;
                            }

                            translations.TryAdd(property.Path, property.Value.ToObject<string>() ?? string.Empty);
                            break;
                        }
                    case JTokenType.Object:
                        foreach (var child in ((JObject)current).Children().Reverse())
                        {
                            stack.Push(child);
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Json tokens of type \"{current.GetType()}\" are not allowed. ({current.ToString(Formatting.None)})");
                    }
                }

                return new Dictionary<string, Dictionary<string, string>> { { langNameNode.Value?.ToString() ?? string.Empty, translations } };
            }

            return null;
        }
        #endregion Deserialize

        #region Serialize
        /// <summary>
        /// Serializes the specified <paramref name="languageName"/> and <paramref name="translations"/> into a JSON string using the single language syntax.
        /// </summary>
        /// <param name="languageName">The name of the language.</param>
        /// <param name="translations">The translation dictionary for the specified <paramref name="languageName"/>.</param>
        /// <param name="formatting">The JSON formatting style to use.</param>
        /// <returns><see cref="string"/> representation of the specified language.</returns>
        public string Serialize(string languageName, IReadOnlyDictionary<string, string> translations, Formatting formatting = Formatting.Indented)
        {
            var root = new JObject();

            foreach (var (key, value) in translations)
            {
                var path = key.Split(Loc.PathSeparator);
                var node = root;
                // enumerate path segments, except for the last one
                for (int i = 0, i_max = path.Length - 1; i < i_max; ++i)
                {
                    var currentPathSegment = path[i];

                    if (node.TryGetValue(currentPathSegment, out var existingSubNode))
                    {
                        if (existingSubNode.Type != JTokenType.Object)
                            throw new InvalidOperationException($"Expected {nameof(JToken)} for path segment \"{currentPathSegment}\" in \"{string.Join(Loc.PathSeparator, path)}\" to be an object (was {existingSubNode.Type:G})! (Something went VERY wrong!)");
                        node = (JObject)existingSubNode;
                    }
                    else
                    {
                        var subNode = new JObject();
                        node.Add(currentPathSegment, subNode);
                        node = subNode;
                    }
                }

                node.Add(path[^1], value);
            }

            return JsonConvert.SerializeObject(root, formatting, JsonSerializerSettings);
        }
        /// <summary>
        /// Serializes the specified <paramref name="singleLanguageDictionary"/> into a JSON string using the single language syntax.
        /// </summary>
        /// <param name="singleLanguageDictionary"></param>
        /// <returns><see cref="string"/> representation of the specified <paramref name="singleLanguageDictionary"/>.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="singleLanguageDictionary"/> contains more than 1 language.</exception>
        public override string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> singleLanguageDictionary)
        {
            if (singleLanguageDictionary.Count > 1)
                throw new InvalidOperationException($"{nameof(JsonSingleTranslationLoader)} cannot serialize multiple languages without specifying a languageName!");
            var (langName, translations) = singleLanguageDictionary.First();
            return Serialize(langName, translations);
        }
        #endregion Serialize
    }
}
