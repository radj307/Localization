using System.ComponentModel;

namespace Localization
{
    /// <summary>
    /// Arguments for events that occur when the current language changes.
    /// </summary>
    public sealed class CurrentLanguageChangedEventArgs : HandledEventArgs
    {
        #region Constructor
        internal CurrentLanguageChangedEventArgs(string oldLanguageName, string newLanguageName)
        {
            OldLanguageName = oldLanguageName;
            NewLanguageName = newLanguageName;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the outgoing current language name.
        /// </summary>
        public string OldLanguageName { get; }
        /// <summary>
        /// Gets the incoming current language name.
        /// </summary>
        public string NewLanguageName { get; }
        #endregion Properties
    }
    /// <summary>
    /// Event that occurs when the current language is changing or has changed.
    /// </summary>
    /// <param name="sender">The <see cref="Loc"/> instance that fired the event.</param>
    /// <param name="e">The <see cref="CurrentLanguageChangedEventArgs"/> instance for this event.</param>
    public delegate void CurrentLanguageChangeEventHandler(object? sender, CurrentLanguageChangedEventArgs e);
}
