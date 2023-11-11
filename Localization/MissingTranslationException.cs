using System;

namespace Localization
{
    /// <summary>
    /// Represents a missing translation error.
    /// </summary>
    public sealed class MissingTranslationException : Exception
    {
        #region Constructor
        internal MissingTranslationException(string languageName, string stringPath)
        {
            LanguageName = languageName;
            StringPath = stringPath;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the name of the language that caused the exception.
        /// </summary>
        public string LanguageName { get; }
        /// <summary>
        /// Gets the missing string path that caused the exception.
        /// </summary>
        public string StringPath { get; }
        #endregion Properties
    }
}
