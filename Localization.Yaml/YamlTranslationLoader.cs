﻿using Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Localization.Yaml
{
    /// <summary>
    /// Default loader for YAML translation config files.
    /// </summary>
    public class YamlTranslationLoader : ITranslationLoader
    {
        #region Properties
        /// <inheritdoc/>
        public string[] SupportedFileExtensions { get; } = new[] { ".yml", ".yaml" };
        #endregion Properties

        #region Methods

        #region Deserialize
        /// <inheritdoc/>
        public Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            var yamlStream = new YamlStream();

            using (var reader = new StringReader(serializedData))
            {
                yamlStream.Load(reader);
            }

            if (yamlStream.Documents.Count == 0) return null;
            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            var dict = new Dictionary<string, Dictionary<string, string>>();
            root.ToList().ForEach(nodePair => ParseElementRecursive(dict, new Stack<string>(), nodePair));

            return dict;
        }
        #endregion Deserialize

        #region (Private) ParseElementRecursive
        private static void ParseElementRecursive(Dictionary<string, Dictionary<string, string>> dict, Stack<string> path, KeyValuePair<YamlNode, YamlNode> nodePair)
        {
            if (nodePair.Value is YamlMappingNode mappingNode)
            { // node
                path.Push(nodePair.Key.ToString());
                mappingNode.ToList().ForEach(subNodePair => ParseElementRecursive(dict, path, subNodePair));
                path.Pop();
            }
            else
            { // value
                var languageName = nodePair.Key.ToString();

                // get or create a language dictionary for this language name
                Dictionary<string, string> languageDictionary;
                if (dict.TryGetValue(languageName, out var existingLangDict))
                    languageDictionary = existingLangDict;
                else
                {
                    languageDictionary = new Dictionary<string, string>();
                    dict.Add(languageName, languageDictionary);
                }

                // add the translated string to the language dictionary
                languageDictionary.Add(string.Join(Loc.PathSeparator, path.Reverse()), nodePair.Value.ToString());
            }
        }
        #endregion (Private) ParseElementRecursive

        #region Serialize
        /// <summary>
        /// Serializes the <paramref name="languageDictionaries"/> using the specified <paramref name="serializer"/>.
        /// </summary>
        /// <param name="languageDictionaries">A dictionary where the keys correspond to the language name, and values are subdictionaries where the keys are the string paths of the corresponding value, and values are the translated string.</param>
        /// <param name="serializer">The YAML serializer instance to use for serializing the data.</param>
        /// <inheritdoc cref="ITranslationLoader.Serialize(IReadOnlyDictionary{string, IReadOnlyDictionary{string, string}})"/>
        public string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries, ISerializer serializer)
        {
            var root = new YamlMappingNode();

            foreach (var (languageName, languageDict) in languageDictionaries)
            {
                foreach (var (path, value) in languageDict)
                {
                    var node = CreateBranch(root, path.Split(Loc.PathSeparator));
                    node.Add(languageName, value);
                }
            }

            return serializer.Serialize(root);
        }
        /// <inheritdoc/>
        public string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries)
            => Serialize(languageDictionaries, new Serializer());
        #endregion Serialize

        #region (Private) CreateBranch
        private static YamlMappingNode CreateBranch(YamlMappingNode root, string[] path)
        {
            YamlMappingNode node = root;
            for (int i = 0, i_max = path.Length; i < i_max; ++i)
            {
                var currentPathSegment = path[i];
                if (node.Children.TryGetValue(currentPathSegment, out var existingSubNode))
                {
                    if (existingSubNode.NodeType != YamlNodeType.Mapping)
                        throw new InvalidOperationException($"Expected {nameof(YamlNode)} for path segment \"{currentPathSegment}\" in \"{string.Join(Loc.PathSeparator, path)}\" to be a Mapping (was {existingSubNode.NodeType:G})!");
                    node = (YamlMappingNode)existingSubNode;
                }
                else
                {
                    var subNode = new YamlMappingNode();
                    node.Add(currentPathSegment, subNode);
                    node = subNode;
                }
            }
            return node;
        }
        #endregion (Private) CreateBranch

        #endregion Methods
    }
}
