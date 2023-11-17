using System.Collections.Generic;

namespace Localization
{
    /// <summary>
    /// Represents an instance that can deserialize strings and serialize language dictionaries.
    /// </summary>
    public interface ITranslationLoader
    {
        #region Properties
        /// <summary>
        /// Gets the list of file type extensions supported by this translation loader instance.
        /// </summary>
        /// <remarks>
        /// File extensions in this list shall not include the <see cref="Loc.ExtensionPrefix"/>, and should include the '.' prefix.
        /// </remarks>
        string[] SupportedFileExtensions { get; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Deserializes the specified <paramref name="serializedData"/> into language dictionaries.
        /// </summary>
        /// <param name="serializedData">A string containing any number of serialized language dictionaries.</param>
        /// <returns>A dictionary where the keys correspond to the language name, and values are subdictionaries where the keys are the paths and values are the translated strings.</returns>
        Dictionary<string, Dictionary<string, string>>? Deserialize(string serializedData);
        /// <summary>
        /// Serializes the specified <paramref name="languageDictionaries"/> into a string.
        /// </summary>
        /// <returns>A dictionary where the keys correspond to the language name, and values are subdictionaries where the keys are the paths and values are the translated strings.</returns>
        /// <returns>A string containing the serialized <paramref name="languageDictionaries"/>.</returns>
        string Serialize(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languageDictionaries);
        #endregion Methods
    }
}
