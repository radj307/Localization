using Localization.Events;
using Localization.Interfaces;
using Localization.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Localization
{
    public class Loc : INotifyPropertyChanged
    {
        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Loc"/> instance.
        /// </summary>
        [Obsolete("Creating a new Loc instance is not recommended. Use the static Loc.Instance property instead.", error: false)]
        public Loc() => _currentLanguageName = string.Empty;
        /// <summary>
        /// Creates a new <see cref="Loc"/> instance with the specified parameters.
        /// </summary>
        /// <param name="languages">Language dictionary.</param>
        /// <param name="currentLanguageName">The current language name.</param>
        public Loc(IReadOnlyObservableConcurrentDictionary<string, LanguageDictionary> languages, string currentLanguageName)
        {
            _languages = new ObservableConcurrentDictionary<string, LanguageDictionary>(languages);
            _currentLanguageName = currentLanguageName;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the <see langword="static"/> <see cref="Loc"/> instance.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public static Loc Instance { get; } = new Loc();
#pragma warning restore CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets the language dictionary that contains all loaded translations.
        /// </summary>
        public IReadOnlyObservableConcurrentDictionary<string, LanguageDictionary> Languages => _languages;
        private readonly ObservableConcurrentDictionary<string, LanguageDictionary> _languages = new ObservableConcurrentDictionary<string, LanguageDictionary>();
        /// <summary>
        /// Gets or sets the name of the current default language to get translations from when no language is explicitly provided.
        /// </summary>
        /// <remarks>
        /// The language specified doesn't have to be loaded when you set this.
        /// </remarks>
        public string CurrentLanguageName
        {
            get => _currentLanguageName;
            set
            {
                if (NotifyCurrentLanguageChanging(_currentLanguageName, value).Handled)
                    return;

                var previousValue = _currentLanguageName;
                _currentLanguageName = value;
                CurrentLanguageDictionary = Languages.TryGetValue(_currentLanguageName, out var dict)
                    ? dict
                    : null;
                NotifyPropertyChanged();
                NotifyCurrentLanguageChanged(previousValue, _currentLanguageName);
            }
        }
        private string _currentLanguageName;
        /// <summary>
        /// Gets the <see cref="LanguageDictionary"/> associated with the CurrentLanguageName.
        /// </summary>
        public LanguageDictionary? CurrentLanguageDictionary { get; private set; }
        /// <summary>
        /// Gets or sets the name of a language to use as a fallback when a translation is missing and no default text was provided.
        /// </summary>
        public string? FallbackLanguageName
        {
            get => _fallbackLanguageName;
            set
            {
                if (NotifyFallbackLanguageChanging(_fallbackLanguageName, value).Handled)
                    return;

                var previousValue = _fallbackLanguageName;
                _fallbackLanguageName = value;
                FallbackLanguageDictionary = _fallbackLanguageName != null && Languages.TryGetValue(_fallbackLanguageName, out var dict)
                    ? dict
                    : null;
                NotifyPropertyChanged();
                NotifyFallbackLanguageChanged(previousValue, _fallbackLanguageName);
            }
        }
        private string? _fallbackLanguageName = null;
        /// <summary>
        /// Gets the <see cref="LanguageDictionary"/> associated with the FallbackLanguageName.
        /// </summary>
        public LanguageDictionary? FallbackLanguageDictionary { get; private set; }
        /// <summary>
        /// Gets the names of all currently loaded languages.
        /// </summary>
        public IReadOnlyObservableCollection<string> AvailableLanguageNames => _availableLanguageNames;
        private readonly ReadOnlyObservableCollection<string> _availableLanguageNames = new ReadOnlyObservableCollection<string>();
        /// <summary>
        /// Gets or sets whether the Translate/Tr methods will use the requested string path as a final fallback. The default value is <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> when the Translate/Tr methods use the string path as a fallback; <see langword="false"/> when they use an empty string instead.</returns>
        public bool UseStringPathAsFallback { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the Translate/Tr methods will throw an exception when the requested string path wasn't found. The default value is <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> when exceptions are thrown when a translation isn't found; otherwise, <see langword="false"/>.</returns>
        public bool ThrowOnMissingTranslation { get; set; } = false;
        /// <summary>
        /// Gets the list of <see cref="ITranslationLoader"/> instances to use when loading translation config files.
        /// </summary>
        public List<ITranslationLoader> TranslationLoaders { get; } = new List<ITranslationLoader>();
        #endregion Properties

        #region Events
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        /// <summary>
        /// Occurs when a translation wasn't found in the requested language.
        /// </summary>
        public event MissingTranslationStringRequestedEventHandler? MissingTranslationStringRequested;
        private void NotifyMissingTranslationStringRequested(string languageName, string stringPath) => MissingTranslationStringRequested?.Invoke(this, new MissingTranslationStringRequestedEventArgs(languageName, stringPath));
        /// <summary>
        /// Occurs when the CurrentLanguageName was changed for any reason.
        /// </summary>
        public event EventHandler<CurrentLanguageChangeEventArgs>? CurrentLanguageChanged;
        private void NotifyCurrentLanguageChanged(string oldLanguageName, string newLanguageName) => CurrentLanguageChanged?.Invoke(this, new CurrentLanguageChangeEventArgs(oldLanguageName, newLanguageName));
        /// <summary>
        /// Occurs prior to the CurrentLanguageName being changed for any reason.
        /// </summary>
        /// <remarks>
        /// Setting the event argument's Handled property to <see langword="true"/> will prevent the current language from changing.
        /// </remarks>
        public event EventHandler<CurrentLanguageChangeEventArgs>? CurrentLanguageChanging;
        private CurrentLanguageChangeEventArgs NotifyCurrentLanguageChanging(string oldLanguageName, string newLanguageName)
        {
            var args = new CurrentLanguageChangeEventArgs(oldLanguageName, newLanguageName);
            CurrentLanguageChanging?.Invoke(this, args);
            return args;
        }
        /// <summary>
        /// Occurs when the FallbackLanguageName was changed for any reason.
        /// </summary>
        public event FallbackLanguageChangeEventHandler? FallbackLanguageChanged;
        private void NotifyFallbackLanguageChanged(string? oldLanguageName, string? newLanguageName) => FallbackLanguageChanged?.Invoke(this, new FallbackLanguageChangeEventArgs(oldLanguageName, newLanguageName));
        /// <summary>
        /// Occurs prior to the FallbackLanguageName being changed for any reason.
        /// </summary>
        /// <remarks>
        /// Setting the event argument's Handled property to <see langword="true"/> will prevent the fallback language from changing.
        /// </remarks>
        public event FallbackLanguageChangeEventHandler? FallbackLanguageChanging;
        private FallbackLanguageChangeEventArgs NotifyFallbackLanguageChanging(string? oldLanguageName, string? newLanguageName)
        {
            var args = new FallbackLanguageChangeEventArgs(oldLanguageName, newLanguageName);
            FallbackLanguageChanging?.Invoke(this, args);
            return args;
        }
        #endregion Events

        #region Methods

        #region ClearLanguages
        /// <summary>
        /// Removes all loaded translations and clears the language dictionary.
        /// </summary>
        public void ClearLanguages(bool clearCurrentLanguage = false, bool clearFallbackLanguage = false)
        {
            for (int i = Languages.Count - 1; i >= 0; --i)
            {
                _languages.Remove(_languages.Keys.ElementAt(i));
            }
            if (clearCurrentLanguage)
                CurrentLanguageName = string.Empty;
            if (clearFallbackLanguage)
                FallbackLanguageName = null;
        }
        #endregion ClearLanguages

        #region AddLanguageDictionary
        /// <summary>
        /// Merges the <paramref name="translatedStrings"/> into the language dictionary with the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to add.</param>
        /// <param name="translatedStrings">The language dictionary to merge into the specified <paramref name="languageName"/>.</param>
        /// <param name="overwriteExisting">When <see langword="true"/> and a translated string already exists, it is replaced; otherwise when <see langword="false"/>, only new strings are added.</param>
        /// <returns></returns>
        public LanguageDictionary AddLanguageDictionary(string languageName, IReadOnlyDictionary<string, string> translatedStrings, bool overwriteExisting = true)
        {
            if (Languages.TryGetValue(languageName, out var existing))
            {
                existing.Merge(translatedStrings, overwriteExisting);
                return existing;
            }
            else
            {
                _languages.Add(languageName, new LanguageDictionary(translatedStrings));
                _availableLanguageNames.Add(languageName);
                return Languages[languageName];
            }
        }
        /// <summary>
        /// Merges the <paramref name="translatedStrings"/> into the language dictionary with the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to add.</param>
        /// <param name="translatedStrings">The language dictionary to merge into the specified <paramref name="languageName"/>.</param>
        /// <param name="overwriteExisting">When <see langword="true"/> and a translated string already exists, it is replaced; otherwise when <see langword="false"/>, only new strings are added.</param>
        /// <returns></returns>
        public LanguageDictionary AddLanguageDictionary(string languageName, LanguageDictionary languageDictionary, bool overwriteExisting = true)
        {
            if (Languages.TryGetValue(languageName, out var existing))
            {
                existing.Merge(languageDictionary, overwriteExisting);
                return existing;
            }
            else
            {
                _languages.Add(languageName, languageDictionary);
                _availableLanguageNames.Add(languageName);
                return Languages[languageName];
            }
        }
        #endregion AddLanguageDictionary

        #region AddLanguageDictionaries
        /// <summary>
        /// Merges the specified <paramref name="languages"/> into the language dictionary.
        /// </summary>
        /// <param name="languages">Dictionary containing any number of languages.</param>
        /// <param name="overwriteExisting">When <see langword="true"/> and a translated string already exists, it is replaced; otherwise when <see langword="false"/>, only new strings are added.</param>
        public void AddLanguageDictionaries(IReadOnlyDictionary<string, LanguageDictionary> languages, bool overwriteExisting = true)
        {
            foreach (var (languageName, languageDict) in languages)
            {
                AddLanguageDictionary(languageName, languageDict, overwriteExisting);
            }
        }
        /// <summary>
        /// Merges the specified <paramref name="languages"/> into the language dictionary.
        /// </summary>
        /// <param name="languages">Dictionary containing any number of languages.</param>
        /// <param name="overwriteExisting">When <see langword="true"/> and a translated string already exists, it is replaced; otherwise when <see langword="false"/>, only new strings are added.</param>
        public void AddLanguageDictionaries(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> languages, bool overwriteExisting = true)
        {
            foreach (var (languageName, languageDict) in languages)
            {
                AddLanguageDictionary(languageName, languageDict, overwriteExisting);
            }
        }
        /// <inheritdoc cref="AddLanguageDictionaries(IReadOnlyDictionary{string, IReadOnlyDictionary{string, string}}, bool)"/>
        public void AddLanguageDictionaries(Dictionary<string, Dictionary<string, string>> languages, bool overwriteExisting = true)
        {
            foreach (var (languageName, languageDict) in languages)
            {
                AddLanguageDictionary(languageName, languageDict, overwriteExisting);
            }
        }
        #endregion AddLanguageDictionaries

        #region AddTranslationLoader
        /// <summary>
        /// Adds the specified <paramref name="loader"/> to the list of TranslationLoaders, if it isn't present already.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to add.</param>
        /// <returns><see langword="true"/> if the <paramref name="loader"/> was successfully added to the list, or was already present; otherwise, <see langword="false"/>.</returns>
        public bool AddTranslationLoader(ITranslationLoader loader)
        {
            if (TranslationLoaders.Contains(loader)) return true;

            TranslationLoaders.Add(loader);
            return true;
        }
        /// <summary>
        /// Creates a new instance of type <typeparamref name="TLoader"/> if one doesn't exist, and adds it to the translation loaders list.
        /// </summary>
        /// <typeparam name="TLoader">A type that implements <see cref="ITranslationLoader"/> and is default-constructible.</typeparam>
        /// <returns>A pre-existing <typeparamref name="TLoader"/> instance when one was found; otherwise, a new <typeparamref name="TLoader"/> instance.</returns>
        public TLoader AddTranslationLoader<TLoader>() where TLoader : ITranslationLoader, new()
        {
            if (TranslationLoaders.Count > 0)
            {
                var loaderType = typeof(TLoader);
                if (TranslationLoaders.FirstOrDefault(tl => tl.GetType().Equals(loaderType)) is TLoader existingLoader)
                    return existingLoader;
            }

            var loader = new TLoader();
            AddTranslationLoader(loader);
            return loader;
        }
        #endregion AddTranslationLoader

        #region GetTranslationLoaderForFile
        /// <summary>
        /// Gets the first translation loader that supports the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">A file name or path to find a translation loader for.</param>
        /// <returns>A <see cref="ITranslationLoader"/> instance that supports the specified <paramref name="filePath"/> if one was added; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="NoTranslationLoadersException">There weren't any translation loaders in the list.</exception>
        public ITranslationLoader? GetTranslationLoaderForFile(string filePath)
        {
            if (TranslationLoaders.Count == 0)
                throw new NoTranslationLoadersException($"No {nameof(ITranslationLoader)} instances were added to the {nameof(TranslationLoaders)} list prior to calling {nameof(Loc)}.{nameof(LoadFromFile)}!");

            foreach (var loader in TranslationLoaders)
            {
                if (loader.CanLoadFile(filePath))
                {
                    return loader;
                }
            }
            return null;
        }
        #endregion GetTranslationLoaderForFile

        #region LoadFromFile
        /// <summary>
        /// Uses the first available TranslationLoader to load a translation config at the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path of a translation config file.</param>
        /// <returns><see langword="true"/> when successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="NoTranslationLoadersException">There weren't any translation loaders in the list.</exception>
        public bool LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            if (GetTranslationLoaderForFile(filePath) is ITranslationLoader loader
                && loader.TryLoadFromFile(filePath, out var dict))
            {
                AddLanguageDictionaries(dict);
                return true;
            }
            else return false;
        }
        #endregion LoadFromFile

        #region LoadFromDirectory
        /// <summary>
        /// Loads all translation configs in the specified directory using the available translation loaders.
        /// </summary>
        /// <param name="directoryPath">The path of a directory to load translation configs from.</param>
        /// <param name="recurse">When <see langword="true"/>, translation configs in subdirectories are also loaded.</param>
        /// <returns>An enumerable list of translation config file paths that weren't loaded because none of the available translation loaders support them.</returns>
        /// <exception cref="NoTranslationLoadersException">There weren't any translation loaders in the list.</exception>
        public IEnumerable<string>? LoadFromDirectory(string directoryPath, bool recurse = false)
        {
            if (!Directory.Exists(directoryPath))
                return null;

            var loadedFilePaths = new List<string>();
            foreach (var filePath in Directory.EnumerateFiles(directoryPath, '*' + ITranslationLoader.ExtensionPrefix + '*', new EnumerationOptions() { RecurseSubdirectories = recurse }))
            {
                if (LoadFromFile(filePath))
                {
                    loadedFilePaths.Add(filePath);
                }
            }
            return loadedFilePaths;
        }
        #endregion LoadFromDirectory

        #region Translate
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the current language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="stringPath"/> wasn't found.</exception>
        public string Translate(string stringPath, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(stringPath, out string translatedString))
            {
                return translatedString;
            }
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, stringPath);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, stringPath);
                return defaultText ?? (UseStringPathAsFallback ? stringPath : string.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the current language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="stringPath"/> wasn't found.</exception>
        public string Translate(string stringPath, StringComparison stringComparison, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(stringPath, stringComparison, out string translatedString))
            {
                return translatedString;
            }
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, stringPath);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, stringPath);
                return defaultText ?? (UseStringPathAsFallback ? stringPath : string.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the specified language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="stringPath"/> wasn't found.</exception>
        public string Translate(string stringPath, string? defaultText, string languageName)
        {
            if (Languages.TryGetValue(languageName, out var dict) && dict.TryGetValue(stringPath, out string translatedString))
            {
                return translatedString;
            }
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, stringPath);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, stringPath);
                return defaultText ?? (UseStringPathAsFallback ? stringPath : string.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the specified language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="stringPath"/> wasn't found.</exception>
        public string Translate(string stringPath, StringComparison stringComparison, string? defaultText, string languageName)
        {
            if (Languages.TryGetValue(languageName, stringComparison, out var dict) && dict.TryGetValue(stringPath, stringComparison, out string translatedString))
            {
                return translatedString;
            }
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, stringPath);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, stringPath);
                return defaultText ?? (UseStringPathAsFallback ? stringPath : string.Empty);
            }
        }
        #endregion Translate

        #region (Static) Tr
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the current language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string stringPath, string? defaultText = null) => Instance.Translate(stringPath, defaultText);
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the current language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string stringPath, StringComparison stringComparison, string? defaultText = null) => Instance.Translate(stringPath, stringComparison, defaultText);
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the specified language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string stringPath, string? defaultText, string languageName) => Instance.Translate(stringPath, defaultText, languageName);
        /// <summary>
        /// Gets the translation for the specified <paramref name="stringPath"/> in the specified language.
        /// </summary>
        /// <param name="stringPath">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <returns>
        /// The translation for the specified <paramref name="stringPath"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="stringPath"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="stringPath"/> when UseStringPathAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string stringPath, StringComparison stringComparison, string? defaultText, string languageName) => Instance.Translate(stringPath, stringComparison, defaultText, languageName);
        #endregion (Static) Tr

        #endregion Methods
    }
}
