using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Localization.Json
{
    /// <summary>
    /// <see cref="ITranslationLoader"/> for JSON files that uses the default syntax.
    /// </summary>
    /// <remarks>
    /// Syntax example:
    /// <code>
    /// {
    ///   "MainWindow": {
    ///     "Text": {
    ///       "English": "Some text",
    ///       "French":  "Un peu de texte"
    ///     }
    ///   }
    /// }
    /// </code>
    /// Key names should not include <see cref="Loc.PathSeparator"/> characters.
    /// </remarks>
    public class JsonTranslationLoader : ITranslationLoader
    {
        #region Constructors
        /// <summary>
        /// Creates a new <see cref="JsonTranslationLoader"/> instance with the specified <paramref name="jsonSerializerSettings"/>.
        /// </summary>
        /// <param name="jsonSerializerSettings">The settings to use when serializing/deserializing JSON data.</param>
        public JsonTranslationLoader(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings;
        }
        /// <summary>
        /// Creates a new <see cref="JsonTranslationLoader"/> instance with the default serializer settings.
        /// </summary>
        public JsonTranslationLoader() : this(new JsonSerializerSettings() { Formatting = Formatting.Indented }) { }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets or sets the settings to use when deserializing JSON data.
        /// </summary>
        public virtual JsonSerializerSettings JsonSerializerSettings { get; set; }
        /// <summary>
        /// ".json" files.
        /// </summary>
        public string[] SupportedFileExtensions { get; } = new string[] { ".json" };
        #endregion Properties

        #region Methods

        #region (Private) GetKey
        private static string GetKey(JProperty jProperty)
        {
            if (!jProperty.Path.Contains(' '))
                return jProperty.Path; //< we can use the JSON path since it doesn't have spaces

            var path = new List<string>();
            for (JProperty? prop = (JProperty?)jProperty.Parent?.Parent; prop != null; prop = (JProperty?)prop.Parent?.Parent)
            {
                path.Add(prop.Name);
            }
            path.Reverse();
            return string.Join(Loc.PathSeparator, path);
        }
        #endregion (Private) GetKey

        #region Deserialize
        /// <summary>
        /// Deserializes the specified JSON data that uses multiple-language syntax.
        /// </summary>
        /// <param name="serializedData">Serialized JSON data that uses the default multi-language syntax.</param>
        /// <returns>The deserialized language dictionaries.</returns>
        /// <exception cref="InvalidOperationException">The JSON data contained an unsupported element type. Only Objects &amp; Strings are supported.</exception>
        public virtual Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            var root = (JObject?)JsonConvert.DeserializeObject(serializedData);
            if (root == null) return null;

            if (root.TryFindChild<JValue>((key, node) => key.Equals("$LanguageName", StringComparison.OrdinalIgnoreCase), out _))
                return null; //< this JSON doc uses single-language syntax; it can't be loaded by this instance.

            var dict = new Dictionary<string, Dictionary<string, string>>();

            // parse tree depth-first
            var stack = new Stack<JToken>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                switch (current.Type)
                {
                case JTokenType.Property:
                    var property = (JProperty)current;
                    if (property.Value is JObject obj)
                    {
                        current = obj;
                        goto case JTokenType.Object;
                    }
                    if (dict.TryGetValue(property.Name, out var existingSubDict))
                    {
                        existingSubDict[GetKey(property)] = property.Value.ToObject<string>() ?? string.Empty;
                    }
                    else
                    {
                        dict.Add(property.Name, new Dictionary<string, string>() { { GetKey(property), property.Value.ToObject<string>() ?? string.Empty } });
                    }
                    break;
                case JTokenType.Object:
                    foreach (var child in ((JObject)current).Children().Reverse()) //< enumerate children in reverse so they get added to the dictionary in the right order
                    {
                        stack.Push(child);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"JSON token type \"{current.GetType()}\" is not allowed!");
                }
            }

            return dict;
        }
        #endregion Deserialize

        #region Serialize
        /// <inheritdoc/>
        public virtual string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> translations, Formatting formatting)
        {
            var root = new JObject();

            foreach (var (languageName, languageDict) in translations)
            {
                foreach (var (path, value) in languageDict)
                {
                    CreateChildBranch(root, path.Split(Loc.PathSeparator)).Add(languageName, value);
                }
            }

            return JsonConvert.SerializeObject(root, formatting);
        }
        /// <inheritdoc/>
        public virtual string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> translations)
            => Serialize(translations, Formatting.Indented);
        #endregion Serialize

        #region (Private) CreateChildBranch
        private static JObject CreateChildBranch(JObject root, string[] path)
        {
            JObject node = root;
            for (int i = 0, i_max = path.Length; i < i_max; ++i)
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
            return node;
        }
        #endregion (Private) CreateChildBranch

        #endregion Methods
    }
}
