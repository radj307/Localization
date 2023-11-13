using System;
using System.Diagnostics;

namespace Localization
{
    /// <summary>
    /// A wrapper for a translated string that includes contextual information about the translation source.
    /// </summary>
    [DebuggerDisplay("Text = {Text}, Source = {Source}, LanguageName = {LanguageName}")]
    public readonly struct TranslationContext
    {
        #region Constructors
        internal TranslationContext(string text, string? languageName, TranslationSource source)
        {
            Text = text;
            LanguageName = languageName;
            Source = source;
        }
        internal TranslationContext(string text, TranslationSource source)
        {
            if (source >= TranslationSource.ExplicitLanguage && source <= TranslationSource.FallbackLanguage)
                throw new ArgumentException($"Translation source \"{source:G}\" indicates a language source, but no language name was provided!", nameof(source));

            Text = text;
            LanguageName = null;
            Source = source;
        }
        #endregion Constructors

        #region Operators
        /// <summary>
        /// Gets the <see cref="Text"/> property from the specified <paramref name="translationContext"/> instance.
        /// </summary>
        /// <param name="translationContext">A <see cref="TranslationContext"/> instance.</param>
        public static implicit operator string(TranslationContext translationContext) => translationContext.Text;
        #endregion Operators

        #region Properties
        /// <summary>
        /// Gets the translated string.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Gets the name of the language that the Text came from.
        /// </summary>
        /// <returns><see langword="null"/> when the Text didn't come from a language.</returns>
        public string? LanguageName { get; }
        /// <summary>
        /// Gets the source that the Text came from.
        /// </summary>
        /// <returns><see cref="TranslationSource"/> value that indicates which source the Text came from.</returns>
        public TranslationSource Source { get; }
        #endregion Properties

        #region (Enum) TranslationSource
        /// <summary>
        /// Defines the sources of a translation.
        /// </summary>
        public enum TranslationSource : byte
        {
            /// <summary>
            /// Translation came a language that was specified explicitly.
            /// </summary>
            /// <remarks>
            /// The language name can be retrieved from the <see cref="TranslationContext.LanguageName"/> property.
            /// </remarks>
            ExplicitLanguage = 0,
            /// <summary>
            /// Translation came from the current language.
            /// </summary>
            /// <remarks>
            /// The language name can be retrieved from the <see cref="TranslationContext.LanguageName"/> property.
            /// </remarks>
            CurrentLanguage = 1,
            /// <summary>
            /// Translation came from the fallback language.
            /// </summary>
            /// <remarks>
            /// The language name can be retrieved from the <see cref="TranslationContext.LanguageName"/> property.
            /// </remarks>
            FallbackLanguage = 2,
            /// <summary>
            /// Text came from the provided defaultText parameter.
            /// </summary>
            DefaultText = 3,
            /// <summary>
            /// Text came from the provided stringPath parameter.
            /// </summary>
            StringPath = 4,
            /// <summary>
            /// Text is an empty string because no other fallback sources were available.
            /// </summary>
            Empty = 5,
        }
        #endregion (Enum) TranslationSource
    }
}
