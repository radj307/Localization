using Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Localization.Xml
{
    /// <summary>
    /// <see cref="ITranslationLoader"/> implementation for XML.
    /// </summary>
    public class XmlTranslationLoader : ITranslationLoader
    {
        #region Properties
        /// <summary>
        /// ".xml" files.
        /// </summary>
        public string[] SupportedFileExtensions { get; } = new string[] { ".xml" };
        #endregion Properties

        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML string.
        /// </summary>
        /// <param name="serializedData">A string containing serialized XML translations.</param>
        /// <inheritdoc/>
        public virtual Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(serializedData);

            var dict = new Dictionary<string, Dictionary<string, string>>();

            var stack = new Stack<XmlNode>();
            stack.Push(doc.DocumentElement);
            while (stack.Count > 0)
            {
                var node = stack.Pop();

                if (node.Value != null)
                { // this is a value "node"
                    var langNode = node.ParentNode;
                    dict.GetOrCreateValue(langNode.Name)
                        .Add(GetPath(langNode), node.Value);
                }
                else if (node.HasChildNodes)
                {
                    foreach (var item in node.ChildNodes.Cast<XmlNode>().Reverse())
                    {
                        stack.Push(item);
                    }
                }
            }

            return dict;
        }
        #endregion Deserialize

        #region (Protected) GetPath
        protected static string GetPath(XmlNode node)
        {
            var path = new List<string>();
            for (XmlNode? n = node; n != null; n = n.ParentNode)
            {
                if (n.NodeType == XmlNodeType.Document)
                    break; //< we've reached the root node, don't include its name "#document"
                path.Add(n.Name);
            }
            path.Reverse();
            return string.Join(Loc.PathSeparator, path);
        }
        #endregion (Protected) GetPath

        #region Serialize
        /// <summary>
        /// Serializes the specified <paramref name="languageDictionaries"/> into an XML string.
        /// </summary>
        /// <inheritdoc/>
        public virtual string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries)
        {
            var doc = new XmlDocument();

            // create xml declaration
            var decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(decl, null);

            XmlNode root = doc;
            
            foreach (var (langName, langDict) in languageDictionaries)
            {
                foreach (var (key, value) in langDict)
                {
                    var path = key.Split(Loc.PathSeparator);

                    if (path.Length > 0)
                    {
                        CreateBranch(doc, root, path[1..]).InnerXml = value;
                    }
                    else throw new InvalidOperationException("Expected a valid path!");
                }
            }

            return doc.InnerXml;
        }
        #endregion Serialize

        #region (Protected) CreateBranch
        protected static XmlNode CreateBranch(XmlDocument doc, XmlNode root, string[] path)
        {
            XmlNode node = root;
            for (int i = 0, i_max = path.Length; i < i_max; ++i)
            {
                var pathSegment = path[i];

                if (node.SelectSingleNode(pathSegment) is XmlNode existingSubNode)
                {
                    node = existingSubNode;
                }
                else
                {
                    var subNode = doc.CreateElement(pathSegment);
                    node.AppendChild(subNode);
                    node = subNode;
                }
            }
            return node;
        }
        #endregion (Protected) CreateBranch
    }
}
