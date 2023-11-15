using Localization.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Localization.Json
{
    /// <summary>
    /// Alternative to <see cref="JsonTranslationLoader"/> that uses a easier to write syntax, with the limitation of only supporting 1 language per file.
    /// </summary>
    public class JsonSingleTranslationLoader : JsonTranslationLoader, ITranslationLoader
    {
        #region Properties
        /// <summary>
        /// ".json" file extensions.
        /// </summary>
        public string[] SupportedFileExtensions => Util.SupportedFileExtensionStrings;
        #endregion Properties

        public override Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            if (JsonConvert.DeserializeObject(serializedData) is JObject root
                && root.TryFindChild<JValue>((key, node) => key.Equals("$LanguageName", StringComparison.OrdinalIgnoreCase), out var langNameNode, out var langNameNodeKey))
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
                            throw new InvalidOperationException($"Expected {nameof(JToken)} for path segment \"{currentPathSegment}\" in \"{string.Join(Loc.PathSeparator, path)}\" to be an object (was {existingSubNode.Type:G})!");
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
        public override string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries)
        {
            if (languageDictionaries.Count > 1)
                throw new InvalidOperationException($"{nameof(JsonSingleTranslationLoader)} cannot serialize multiple languages without specifying a languageName!");
            var (langName, translations) = languageDictionaries.First();
            return Serialize(langName, translations);
        }
    }
}
