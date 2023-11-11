using System.ComponentModel;

namespace Localization.Events
{
    /// <summary>
    /// Arguments for events that occur when the current language changes.
    /// </summary>
    public sealed class CurrentLanguageChangeEventArgs : HandledEventArgs
    {
        #region Constructor
        internal CurrentLanguageChangeEventArgs(string oldLanguageName, string newLanguageName)
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
    public delegate void CurrentLanguageChangeEventHandler(Loc instance, CurrentLanguageChangeEventArgs e);
}
