using Localization.Events;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Localization.WPF
{
    internal class TranslationBinding : INotifyPropertyChanged, IDisposable
    {
        #region Constructor
        public TranslationBinding(string path)
        {
            Path = path;

            AttachEventHandlers();
        }
        #endregion Constructor

        #region Fields
        private TranslationContext.TranslationSource textSource;
        #endregion Fields

        #region Properties
        public string Text
        {
            get
            {
                var context = GetTranslatedString();
                textSource = context.Source;
                return Prefix + context.Text + Suffix;
            }
        }
        public string Path { get; set; }
        public string? DefaultText { get; set; }
        public string? LanguageName { get; set; }
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        #endregion Properties

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        #endregion Events

        #region GetTranslatedString
        public TranslationContext GetTranslatedString()
        {
            if (StringComparison != StringComparison.Ordinal)
            {
                if (LanguageName == null)
                    return Loc.ContextTr(Path, StringComparison, DefaultText);
                return Loc.ContextTr(Path, StringComparison, DefaultText, LanguageName);
            }
            else
            {
                if (LanguageName == null)
                    return Loc.ContextTr(Path, DefaultText);
                return Loc.ContextTr(Path, DefaultText, LanguageName);
            }
        }
        #endregion GetTranslatedString

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

        #region EventHandlers
        private void Instance_CurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
        {
            if (textSource == TranslationContext.TranslationSource.ExplicitLanguage)
                return;

            NotifyPropertyChanged(nameof(Text));
        }
        private void Instance_FallbackLanguageChanged(object? sender, FallbackLanguageChangedEventArgs e)
        {
            if (textSource >= TranslationContext.TranslationSource.FallbackLanguage)
                return;

            NotifyPropertyChanged(nameof(Text));
        }
        #endregion EventHandlers

        #region IDisposable
        /// <inheritdoc cref="Dispose"/>
        ~TranslationBinding() => Dispose();
        /// <summary>
        /// Detatches event handlers from the Loc instance.
        /// </summary>
        public void Dispose()
        {
            DetatchEventHandlers();
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
