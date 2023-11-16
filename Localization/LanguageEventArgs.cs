using System;

namespace Localization
{
    /// <summary>
    /// Event arguments for language added/removed events.
    /// </summary>
    public sealed class LanguageEventArgs : EventArgs
    {
        #region Constructors
        internal LanguageEventArgs(string languageName, TranslationDictionary translations)
        {
            LanguageName = languageName;
            Translations = translations;
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the name of the language that triggered the event.
        /// </summary>
        public string LanguageName { get; }
        /// <summary>
        /// Gets the translation dictionary of the language that triggered the event.
        /// </summary>
        public TranslationDictionary Translations { get; }
        #endregion Properties
    }
    /// <summary>
    /// Handler type for language added/removed events.
    /// </summary>
    /// <param name="sender">The <see cref="Loc"/> instance that triggered the event.</param>
    /// <param name="e">The <see cref="LanguageEventArgs"/> instance for the event.</param>
    public delegate void LanguageEventHandler(object? sender, LanguageEventArgs e);
}
