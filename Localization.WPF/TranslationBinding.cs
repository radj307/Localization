using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace Localization.WPF
{
    internal class TranslationBinding : INotifyPropertyChanged, IDisposable
    {
        #region Constructor
        public TranslationBinding(TrExtension trExtension)
        {
            owner = trExtension;

            AttachEventHandlers();
        }
        #endregion Constructor

        #region Fields
        private readonly TrExtension owner;
        private static readonly Regex _getFormatStringReplacementSections = new(@"(?:\[(\d+)(?:(?:,\d+){0,1}(?::[^\s]+){0,1})\]|{(\d+)(?:(?:,\d+){0,1}(?::[^\s]+){0,1})})", RegexOptions.Compiled);
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets the formatted translated text.
        /// </summary>
        public string Text
        {
            get => owner.Prefix + GetTranslatedAndFormattedString() + owner.Suffix;
        }
        #endregion Properties

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        #endregion Events

        #region Methods

        #region (Private) GetTranslatedAndFormattedString
        private string GetTranslatedAndFormattedString()
        {
            if (owner.FormatString != null)
            {
                bool foundArg0 = false; //< required since out params cant be set in lambdas
                var formatString = _getFormatStringReplacementSections.Replace(owner.FormatString, m =>
                {
                    string s;
                    if (m.Groups[1].Success)
                    { // valid replacement expression with square brackets; replace them with regular brackets
                        s = m.Groups[1].Value;
                        if (!foundArg0 && s.Equals("0", StringComparison.Ordinal))
                            foundArg0 = true;
                        return $"{{{s}}}";
                    }
                    else if (m.Groups[2].Success)
                    { // valid replacement expression
                        s = m.Groups[2].Value;
                        if (!foundArg0 && s.Equals("0", StringComparison.Ordinal))
                            foundArg0 = true;
                        return $"{{{s}}}";
                    }
                    else return m.Value; //< invalid replacement expression; escape the brackets so string.Format doesn't throw
                });
                string translatedString = foundArg0 ? GetTranslatedString(owner) : string.Empty;

                // build the argument array
                object?[] args = new object?[1 + (owner.FormatArgs?.Length ?? 0)];
                args[0] = translatedString;
                if (owner.FormatArgs != null)
                { // add the additional format args
                    for (int i = 0, i_max = owner.FormatArgs.Length; i < i_max; ++i)
                    {
                        args[1 + i] = owner.FormatArgs[i];
                    }
                }

                try
                {
                    return string.Format(formatString, args);
                }
                catch (FormatException)
                {
                    return translatedString;
                }
            }
            else return GetTranslatedString(owner);
        }
        #endregion (Private) GetTranslatedAndFormattedString

        #region (Private) GetTranslatedString
        private static string GetTranslatedString(TrExtension owner)
        {
            if (owner.Key == null) return owner.DefaultText ?? string.Empty;

            if (owner.StringComparison != StringComparison.Ordinal)
            {
                if (owner.LanguageName == null)
                    return Loc.Tr(owner.Key, owner.StringComparison, owner.DefaultText);
                return Loc.Tr(owner.Key, owner.StringComparison, owner.DefaultText, owner.LanguageName);
            }
            else
            {
                if (owner.LanguageName == null)
                    return Loc.Tr(owner.Key, owner.DefaultText);
                return Loc.Tr(owner.Key, owner.DefaultText, owner.LanguageName);
            }
        }
        #endregion (Private) GetTranslatedString

        #region AttachEventHandlers
        private void AttachEventHandlers()
        {
            WeakEventManager<Loc, CurrentLanguageChangedEventArgs>.AddHandler(Loc.Instance, nameof(Loc.CurrentLanguageChanged), Instance_CurrentLanguageChanged);
            WeakEventManager<Loc, FallbackLanguageChangedEventArgs>.AddHandler(Loc.Instance, nameof(Loc.FallbackLanguageChanged), Instance_FallbackLanguageChanged);
        }
        #endregion AttachEventHandlers

        #region DetatchEventHandlers
        private void DetatchEventHandlers()
        {
            WeakEventManager<Loc, CurrentLanguageChangedEventArgs>.RemoveHandler(Loc.Instance, nameof(Loc.CurrentLanguageChanged), Instance_CurrentLanguageChanged);
            WeakEventManager<Loc, FallbackLanguageChangedEventArgs>.RemoveHandler(Loc.Instance, nameof(Loc.FallbackLanguageChanged), Instance_FallbackLanguageChanged);
        }
        #endregion DetatchEventHandlers

        #endregion Methods

        #region EventHandlers

        #region Loc.Instance
        private void Instance_CurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Text));
        }
        private void Instance_FallbackLanguageChanged(object? sender, FallbackLanguageChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Text));
        }
        #endregion Loc.Instance

        #endregion EventHandlers

        #region IDisposable
        /// <inheritdoc cref="Dispose"/>
        ~TranslationBinding() => Dispose();
        /// <summary>
        /// Detatches event handlers from the Loc instance.
        /// </summary>
        /// <remarks>
        /// Calling this is <b>not required</b>, since the event handlers are attached via the <see cref="WeakEventManager"/>.
        /// </remarks>
        public void Dispose()
        {
            DetatchEventHandlers();
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
