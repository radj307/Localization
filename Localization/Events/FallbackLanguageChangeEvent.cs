using System.ComponentModel;

namespace Localization.Events
{
    /// <summary>
    /// Arguments for events that occur when the fallback language changes.
    /// </summary>
    public sealed class FallbackLanguageChangeEventArgs : HandledEventArgs
    {
        #region Constructor
        internal FallbackLanguageChangeEventArgs(string? oldLanguageName, string? newLanguageName)
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
    public delegate void FallbackLanguageChangeEventHandler(Loc instance, FallbackLanguageChangeEventArgs e);
}
