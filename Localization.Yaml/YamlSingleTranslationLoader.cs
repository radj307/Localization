using Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Localization.Yaml
{
    /// <summary>
    /// <see cref="ITranslationLoader"/> for YAML files that uses an alternative syntax that is easier to read &amp; write, but has the limitation of only supporting 1 language per file.
    /// </summary>
    /// <remarks>
    /// Syntax example:
    /// <code>
    /// $LanguageName: English
    /// 
    /// MainWindow:
    ///   Text: Some text
    ///   Other: "some other text"
    /// </code>
    /// </remarks>
    public class YamlSingleTranslationLoader : YamlTranslationLoader, ITranslationLoader
    {
        #region Deserialize
        /// <inheritdoc/>
        public virtual Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            var yamlStream = new YamlStream();
            using (var reader = new StringReader(serializedData))
            {
                yamlStream.Load(reader);
            }

            if (yamlStream.Documents.Count == 0) return null;
            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            // get the language name node
            string? languageName = null;
            foreach (var (k, v) in root)
            {
                if (k.ToString().Equals("$LanguageName", StringComparison.OrdinalIgnoreCase))
                {
                    languageName = v.ToString();
                    root.Children.Remove(k); //< don't include $LanguageName in translations
                    break;
                }
            }
            if (languageName == null) return null;

            // parse the document
            var dict = new Dictionary<string, string>();
            foreach (var (k, v) in root)
                ParseElementRecursive(dict, new Stack<string>(), k.ToString(), v);

            return new Dictionary<string, Dictionary<string, string>>() { { languageName, dict } };
        }
        #endregion Deserialize

        #region (Private) ParseElementRecursive
        private static void ParseElementRecursive(Dictionary<string, string> dict, Stack<string> path, string key, YamlNode node)
        {
            path.Push(key);
            if (node is YamlMappingNode mappingNode)
            { // node
                foreach (var (k, v) in mappingNode)
                {
                    ParseElementRecursive(dict, path, k.ToString(), v); //< RECURSE
                }
            }
            else
            { // value
                // add this translation to the dictionary
                dict.Add(
                    key: string.Join(Loc.PathSeparator, path.Reverse()),
                    value: node.ToString());
            }
            path.Pop();
        }
        #endregion (Private) ParseElementRecursive

        #region Serialize
        public override string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries, ISerializer serializer)
        {
            if (languageDictionaries.Count > 1)
                throw new InvalidOperationException($"{nameof(YamlSingleTranslationLoader)} does not support serializing multiple languages!");
            else if (languageDictionaries.Count == 0)
                return string.Empty;

            var langDict = languageDictionaries.First().Value;
            var root = new YamlMappingNode();

            foreach (var (key, value) in langDict)
            {
                var path = key.Split(Loc.PathSeparator);
                var node = CreateBranch(root, path[..^1]);
                node.Add(path[^1], value);
            }

            return serializer.Serialize(root);
        }
        #endregion Serialize
    }
}
