using System.ComponentModel;

namespace Localization
{
    /// <summary>
    /// Arguments for events that occur when the fallback language changes.
    /// </summary>
    public sealed class FallbackLanguageChangedEventArgs : HandledEventArgs
    {
        #region Constructor
        internal FallbackLanguageChangedEventArgs(string? oldLanguageName, string? newLanguageName)
        {
            OldLanguageName = oldLanguageName;
            NewLanguageName = newLanguageName;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the outgoing fallback language name.
        /// </summary>
        public string? OldLanguageName { get; }
        /// <summary>
        /// Gets the incoming fallback language name.
        /// </summary>
        public string? NewLanguageName { get; }
        #endregion Properties
    }
    /// <summary>
    /// Event that occurs when the fallback language is changing or has changed.
    /// </summary>
    /// <param name="sender">The <see cref="Loc"/> instance that fired the event.</param>
    /// <param name="e">The <see cref="FallbackLanguageChangedEventArgs"/> instance for this event.</param>
    public delegate void FallbackLanguageChangeEventHandler(object? sender, FallbackLanguageChangedEventArgs e);
}
