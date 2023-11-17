using System;

namespace Localization
{
    /// <summary>
    /// Represents an error that occurs when no <see cref="ITranslationLoader"/> instances were added to <see cref="Loc.TranslationLoaders"/> prior to calling a method that attempts to get one.<br/>
    /// You have to add translation loaders to <see cref="Loc.TranslationLoaders"/> prior to trying to use them.
    /// </summary>
    public sealed class EmptyTranslationLoadersListException : Exception
    {
        internal EmptyTranslationLoadersListException() { }
        internal EmptyTranslationLoadersListException(string message) : base(message) { }
        internal EmptyTranslationLoadersListException(string message, Exception innerException) : base(message, innerException) { }
    }
}
