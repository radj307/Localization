using System;
using System.Collections.Generic;

namespace Localization
{
    /// <summary>
    /// Event arguments that contain the missing translation string's path and the name of the language.
    /// </summary>
    public sealed class MissingTranslationStringRequestedEventArgs : EventArgs
    {
        #region Constructor
        internal MissingTranslationStringRequestedEventArgs(string languageName, IEnumerable<string> keys)
        {
            LanguageName = languageName;
            Keys = keys;
        }
        internal MissingTranslationStringRequestedEventArgs(string languageName, string key)
        {
            LanguageName = languageName;
            Key = key;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the name of the language that is missing a translation.
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

        #region Methods
        /// <summary>
        /// Gets the name of the missing Key or Keys.
        /// </summary>
        /// <remarks>
        /// When multiple keys were provided, they are joined with ", " as a separator.
        /// </remarks>
        /// <returns>The <see cref="string"/> representation of this instance's Key or Keys.</returns>
        public override string ToString() => Key ?? string.Join(", ", Keys);
        #endregion Methods
    }
    /// <summary>
    /// Event that occurs when a translation was requested but the specified key doesn't exist in the current language.
    /// </summary>
    /// <param name="sender">The <see cref="Loc"/> instance that fired the event.</param>
    /// <param name="e">The <see cref="MissingTranslationStringRequestedEventArgs"/> instance for this event.</param>
    public delegate void MissingTranslationStringRequestedEventHandler(object sender, MissingTranslationStringRequestedEventArgs e);
}
