using Localization.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Localization
{
    /// <summary>
    /// Extension methods for types that implement <see cref="ITranslationLoader"/>.
    /// </summary>
    public static class TranslationLoaderExtensions
    {
        #region Methods

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
            var extensionPrefixPos = name.IndexOf(ITranslationLoader.ExtensionPrefix);
            if (extensionPrefixPos == -1)
                return false;

            var fileExtension = name[(extensionPrefixPos + ITranslationLoader.ExtensionPrefix.Length)..];
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

            return loader.Deserialize(File.ReadAllText(filePath));
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
