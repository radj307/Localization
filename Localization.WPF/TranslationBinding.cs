﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        #endregion Fields

        #region Properties
        public string Text
        {
            get => Prefix + GetTranslatedString() + Suffix;
        }
        public string Key => owner.Key;
        public string? DefaultText => owner.DefaultText;
        public string? LanguageName => owner.LanguageName;
        public StringComparison StringComparison => owner.StringComparison;
        public string Prefix => owner.Prefix;
        public string Suffix => owner.Suffix;
        #endregion Properties

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        #endregion Events

        #region GetTranslatedString
        public string GetTranslatedString()
        {
            if (StringComparison != StringComparison.Ordinal)
            {
                if (LanguageName == null)
                    return Loc.Tr(Key, StringComparison, DefaultText);
                return Loc.Tr(Key, StringComparison, DefaultText, LanguageName);
            }
            else
            {
                if (LanguageName == null)
                    return Loc.Tr(Key, DefaultText);
                return Loc.Tr(Key, DefaultText, LanguageName);
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
            NotifyPropertyChanged(nameof(Text));
        }
        private void Instance_FallbackLanguageChanged(object? sender, FallbackLanguageChangedEventArgs e)
        {
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
