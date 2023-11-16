using Localization.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization.Json
{
    /// <summary>
    /// Default loader for JSON translation config files that supports multiple languages in the same file.
    /// </summary>
    /// <remarks>
    /// Example syntax:
    /// <code>
    /// {
    ///   "MainWindow": {
    ///     "Text": {
    ///       "English": "Some text"
    ///     }
    ///   }
    /// }
    /// </code>
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

        #region Deserialize
        /// <inheritdoc/>
        public virtual Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            var root = (JObject?)JsonConvert.DeserializeObject(serializedData);
            if (root == null) return null;

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
                        existingSubDict[property.Parent!.Path] = property.Value.ToObject<string>() ?? string.Empty;
                    }
                    else
                    {
                        dict.Add(property.Name, new Dictionary<string, string>() { { property.Parent!.Path, property.Value.ToObject<string>() ?? string.Empty } });
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
