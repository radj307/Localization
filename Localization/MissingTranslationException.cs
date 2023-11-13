using System;
using System.Collections.Generic;

namespace Localization
{
    /// <summary>
    /// Represents a missing translation error.
    /// </summary>
    public sealed class MissingTranslationException : Exception
    {
        #region Constructor
        internal MissingTranslationException(string languageName, IEnumerable<string> keys)
        {
            LanguageName = languageName;
            Keys = keys;
        }
        internal MissingTranslationException(string languageName, string key)
        {
            LanguageName = languageName;
            Key = key;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the name of the language that caused the exception.
        /// </summary>
        public string LanguageName { get; }
        /// <summary>
        /// Gets the missing translation keys.
        /// </summary>
        /// <returns>The missing keys when multiple keys were provided; otherwise when a single key was provided, <see langword="null"/>.</returns>
        public IEnumerable<string>? Keys { get; }
        /// <summary>
        /// Gets the missing translation key.
        /// </summary>
        /// <returns>The missing key <see cref="string"/> when a single key was provided; otherwise when multiple keys were provided, <see langword="null"/>.</returns>
        public string? Key { get; }
        #endregion Properties
    }
}
