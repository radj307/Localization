using System;

namespace Localization.Events
{
    /// <summary>
    /// Event arguments that contain the missing translation string's path and the name of the language.
    /// </summary>
    public sealed class MissingTranslationStringRequestedEventArgs : EventArgs
    {
        #region Constructor
        internal MissingTranslationStringRequestedEventArgs(string languageName, string stringPath)
        {
            LanguageName = languageName;
            StringPath = stringPath;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the name of the language that is missing a translation.
        /// </summary>
        public string LanguageName { get; }
        /// <summary>
        /// Gets the path of the missing translation string.
        /// </summary>
        public string StringPath { get; }
        #endregion Properties
    }
    public delegate void MissingTranslationStringRequestedEventHandler(object sender, MissingTranslationStringRequestedEventArgs e);
}
