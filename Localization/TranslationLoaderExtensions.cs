using Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Localization
{
    /// <summary>
    /// Extension methods for types that implement <see cref="ITranslationLoader"/>.
    /// </summary>
    public static class TranslationLoaderExtensions
    {
        #region Methods

        #region Serialize
        /// <inheritdoc cref="ITranslationLoader.Serialize(IReadOnlyDictionary{string, IReadOnlyDictionary{string, string}})"/>
        public static string Serialize(this ITranslationLoader loader, IReadOnlyDictionary<string, Dictionary<string, string>> languageDictionaries)
        {
            return loader.Serialize(languageDictionaries.AsReadOnlyDictionary());
        }
        #endregion Serialize

        #region ConflictsWith
        /// <summary>
        /// Checks if the <see cref="ITranslationLoader"/> instance conflicts with the specified <paramref name="otherLoader"/> by comparing their supported file extensions.
        /// </summary>
        /// <remarks>
        /// A translation loader is conflicting when it doesn't support any unique file extensions.<br/>
        /// For example, a loader than supports ".json" extensions will conflict with a loader that supports ".json" &amp; ".yaml" extensions, but not the other way around unless <paramref name="allowPartialConflicts"/> is <see langword="false"/>.
        /// </remarks>
        /// <param name="loader">An <see cref="ITranslationLoader"/> instance.</param>
        /// <param name="otherLoader">Another <see cref="ITranslationLoader"/> instance to check for conflicts with.</param>
        /// <param name="allowPartialConflicts">When <see langword="false"/>, any matching supported extensions will be considered conflicting; otherwise, when <see langword="true"/> all extensions must match those supported by the <paramref name="otherLoader"/>.</param>
        /// <returns><see langword="true"/> when <paramref name="loader"/> conflicts with the <paramref name="otherLoader"/>; otherwise, <see langword="false"/>.</returns>
        public static bool ConflictsWith(this ITranslationLoader loader, ITranslationLoader otherLoader, bool allowPartialConflicts = true)
        {
            int conflictCount = 0;
            int i_max = loader.SupportedFileExtensions.Length;
            for (int i = 0; i < i_max; ++i)
            {
                // get ONLY the extension
                var ext = loader.SupportedFileExtensions[i].TrimStart('.');
                if (ext.Contains('.')) ext = Path.GetExtension(ext);

                for (int j = 0, j_max = otherLoader.SupportedFileExtensions.Length; j < j_max; ++j)
                {
                    // get ONLY the extension
                    var otherExt = otherLoader.SupportedFileExtensions[j].TrimStart('.');
                    if (otherExt.Contains('.')) otherExt = Path.GetExtension(otherExt);

                    if (ext.Equals(otherExt, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!allowPartialConflicts) return true;
                        ++conflictCount;
                    }
                }
            }
            return conflictCount == i_max;
        }
        #endregion ConflictsWith

        #region CanLoadFile
        /// <summary>
        /// Checks if the translation loader supports files with the specified <paramref name="fileName"/> by checking the supported file extensions.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to check.</param>
        /// <param name="fileName">The file name or path to check.</param>
        /// <returns><see langword="true"/> when the loader supports <paramref name="fileName"/>; otherwise, <see langword="false"/>.</returns>
        public static bool CanLoadFile(this ITranslationLoader loader, string fileName)
        {
            var name = Path.GetFileName(fileName);
            var extensionPrefixStart = name.IndexOf(Loc.ExtensionPrefix);
            if (extensionPrefixStart == -1)
                return false;

            var extensionPrefixEnd = extensionPrefixStart + Loc.ExtensionPrefix.Length;
            if (extensionPrefixEnd >= name.Length)
                return loader.SupportedFileExtensions.Contains(string.Empty);

            var fileExtension = name[extensionPrefixEnd..];
            if (fileExtension.Length == 0)
                return false;

            for (int i = 0, i_max = loader.SupportedFileExtensions.Length; i < i_max; ++i)
            {
                var ext = loader.SupportedFileExtensions[i];
                if ((!ext.StartsWith('.') && ext.Equals(fileExtension[1..])) || ext.Equals(fileExtension))
                    return true;
            }
            return false;
        }
        #endregion CanLoadFile

        #region LoadFromFile
        /// <summary>
        /// Loads translations from the specified <paramref name="filePath"/>.
        /// </summary>
        /// <remarks>
        /// It is recommended to call the CanLoadFile method to check if the loader supports the specified <paramref name="filePath"/> before attempting to load it.
        /// Failure to do so will probably cause an exception if the file type isn't supported.
        /// </remarks>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to use.</param>
        /// <param name="filePath">The path to a translation config file to load.</param>
        /// <returns>The translation strings in the file when successful; otherwise, <see langword="null"/>.</returns>
        public static Dictionary<string, Dictionary<string, string>>? LoadFromFile(this ITranslationLoader loader, string filePath)
        {
            if (!File.Exists(filePath)) return null;

            return loader.Deserialize(File.ReadAllText(filePath, System.Text.Encoding.UTF8));
        }
        #endregion LoadFromFile

        #region TryLoadFromFile
        /// <summary>
        /// Attempts to load translations from the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to use.</param>
        /// <param name="filePath">The path to a translation config file to load.</param>
        /// <param name="translations">The translation strings in the file when the method returns <see langword="true"/>; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when successful and <paramref name="translations"/> is not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryLoadFromFile(this ITranslationLoader loader, string filePath, out Dictionary<string, Dictionary<string, string>> translations)
        {
            try
            {
                translations = loader.LoadFromFile(filePath)!;
                return translations != null;
            }
            catch
            {
                translations = null!;
                return false;
            }
        }
        #endregion TryLoadFromFile

        #endregion Methods
    }
}
