using Localization.Interfaces;
using Localization.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Localization
{
    /// <summary>
    /// The main class in the radj307.Localization package.
    /// Contains the loaded languages &amp; translation loaders, and provides events and methods for interacting with the translations.<br/>
    /// Also provides <see langword="static"/> Tr translation methods that use the default <see cref="Instance"/>.
    /// </summary>
    /// <remarks>
    /// It is recommended to use the <see langword="static"/> <see cref="Instance"/> instance so translations will be available globally.
    /// </remarks>
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

        #region Fields
        /// <summary>
        /// The extension prefix for translation config files.
        /// </summary>
        public const string ExtensionPrefix = ".loc";
        /// <summary>
        /// The separator character for string paths.
        /// </summary>
        public const char PathSeparator = '.';
        #endregion Fields

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
                CurrentLanguageDictionary = _currentLanguageName != null && Languages.TryGetValue(_currentLanguageName, out var dict)
                    ? dict
                    : null;
                NotifyPropertyChanged();
                NotifyCurrentLanguageChanged(previousValue, _currentLanguageName!);
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
        /// Gets or sets whether the Translate/Tr methods will use the requested string path as a final fallback. Defaults to <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> when the Translate/Tr methods use the string path as a fallback; <see langword="false"/> when they use an empty string instead.</returns>
        public bool UseKeyAsFallback { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the Translate/Tr methods will throw an exception when the requested string path wasn't found. Defaults to <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> when exceptions are thrown when a translation isn't found; otherwise, <see langword="false"/>.</returns>
        public bool ThrowOnMissingTranslation { get; set; } = false;
        /// <summary>
        /// Gets or sets whether MissingTranslationStringRequested events are enabled. Defaults to <see langword="true"/>.
        /// </summary>
        public bool NotifyOnMissingTranslation { get; set; } = true;
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
        private void NotifyMissingTranslationStringRequested(string languageName, IEnumerable<string> keys)
        {
            if (!NotifyOnMissingTranslation) return;
            MissingTranslationStringRequested?.Invoke(this, new MissingTranslationStringRequestedEventArgs(languageName, keys));
        }
        private void NotifyMissingTranslationStringRequested(string languageName, string key)
        {
            if (!NotifyOnMissingTranslation) return;
            MissingTranslationStringRequested?.Invoke(this, new MissingTranslationStringRequestedEventArgs(languageName, key));
        }
        /// <summary>
        /// Occurs when the CurrentLanguageName was changed for any reason.
        /// </summary>
        public event CurrentLanguageChangeEventHandler? CurrentLanguageChanged;
        private void NotifyCurrentLanguageChanged(string oldLanguageName, string newLanguageName) => CurrentLanguageChanged?.Invoke(this, new CurrentLanguageChangedEventArgs(oldLanguageName, newLanguageName));
        /// <summary>
        /// Occurs prior to the CurrentLanguageName being changed for any reason.
        /// </summary>
        /// <remarks>
        /// Setting the event argument's Handled property to <see langword="true"/> will prevent the current language from changing.
        /// </remarks>
        public event CurrentLanguageChangeEventHandler? CurrentLanguageChanging;
        private CurrentLanguageChangedEventArgs NotifyCurrentLanguageChanging(string oldLanguageName, string newLanguageName)
        {
            var args = new CurrentLanguageChangedEventArgs(oldLanguageName, newLanguageName);
            CurrentLanguageChanging?.Invoke(this, args);
            return args;
        }
        /// <summary>
        /// Occurs when the FallbackLanguageName was changed for any reason.
        /// </summary>
        public event FallbackLanguageChangeEventHandler? FallbackLanguageChanged;
        private void NotifyFallbackLanguageChanged(string? oldLanguageName, string? newLanguageName) => FallbackLanguageChanged?.Invoke(this, new FallbackLanguageChangedEventArgs(oldLanguageName, newLanguageName));
        /// <summary>
        /// Occurs prior to the FallbackLanguageName being changed for any reason.
        /// </summary>
        /// <remarks>
        /// Setting the event argument's Handled property to <see langword="true"/> will prevent the fallback language from changing.
        /// </remarks>
        public event FallbackLanguageChangeEventHandler? FallbackLanguageChanging;
        private FallbackLanguageChangedEventArgs NotifyFallbackLanguageChanging(string? oldLanguageName, string? newLanguageName)
        {
            var args = new FallbackLanguageChangedEventArgs(oldLanguageName, newLanguageName);
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
            _availableLanguageNames.Clear();
            if (clearCurrentLanguage)
                CurrentLanguageName = string.Empty;
            if (clearFallbackLanguage)
                FallbackLanguageName = null;
        }
        #endregion ClearLanguages

        #region AddLanguage
        /// <summary>
        /// Merges the <paramref name="translations"/> into the language dictionary with the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to add.</param>
        /// <param name="translations">The language dictionary to merge into the specified <paramref name="languageName"/>.</param>
        /// <param name="overwriteExisting">When <see langword="true"/> and a translated string already exists, it is replaced; otherwise when <see langword="false"/>, only new strings are added.</param>
        /// <returns>The merged <see cref="LanguageDictionary"/>.</returns>
        public LanguageDictionary AddLanguage(string languageName, IReadOnlyDictionary<string, string> translations, bool overwriteExisting = true)
        {
            if (Languages.TryGetValue(languageName, out var existing))
            {
                existing.Merge(translations, overwriteExisting);
                return existing;
            }
            else
            {
                _languages.Add(languageName, new LanguageDictionary(translations));
                _availableLanguageNames.Add(languageName);
                return Languages[languageName];
            }
        }
        /// <inheritdoc cref="AddLanguage(string, IReadOnlyDictionary{string, string}, bool)"/>
        public LanguageDictionary AddLanguage(string languageName, LanguageDictionary translations, bool overwriteExisting = true)
        {
            if (Languages.TryGetValue(languageName, out var existing))
            {
                existing.Merge(translations, overwriteExisting);
                return existing;
            }
            else
            {
                _languages.Add(languageName, translations);
                _availableLanguageNames.Add(languageName);
                return Languages[languageName];
            }
        }
        #endregion AddLanguage

        #region ReplaceLanguage
        /// <summary>
        /// Replaces all translations for the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to replace.</param>
        /// <param name="newTranslations">The new translations for the language.</param>
        public void ReplaceLanguage(string languageName, LanguageDictionary newTranslations)
        {
            RemoveLanguage(languageName);
            AddLanguage(languageName, newTranslations);
        }
        #endregion ReplaceLanguage

        #region RemoveLanguage
        /// <summary>
        /// Removes the translations for the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public bool RemoveLanguage(string languageName)
            => _languages.Remove(languageName);
        #endregion RemoveLanguage

        #region TakeLanguage
        /// <summary>
        /// Gets and removes the translations for the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to take.</param>
        /// <returns>The <see cref="LanguageDictionary"/> for the specified <paramref name="languageName"/> if found; otherwise, <see langword="null"/>.</returns>
        public LanguageDictionary? TakeLanguage(string languageName)
        {
            if (Languages.TryGetValue(languageName, out var translations))
            {
                _languages.Remove(languageName);
                return translations;
            }
            return null;
        }
        #endregion TakeLanguage

        #region ChangeLanguageName
        /// <summary>
        /// Changes the specified <paramref name="languageName"/> to <paramref name="newName"/>.
        /// </summary>
        /// <param name="languageName">The name of the language to change the name of.</param>
        /// <param name="newName">The new name to give to the language.</param>
        /// <returns><see langword="true"/> when the name was successfully changed; otherwise, <see langword="false"/>.</returns>
        public bool ChangeLanguageName(string languageName, string newName)
        {
            if (TakeLanguage(languageName) is LanguageDictionary translations)
            {
                AddLanguage(newName, (IReadOnlyDictionary<string, string>)translations);
                return true;
            }
            return false;
        }
        #endregion ChangeLanguageName

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
                AddLanguage(languageName, languageDict, overwriteExisting);
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
                AddLanguage(languageName, languageDict, overwriteExisting);
            }
        }
        /// <inheritdoc cref="AddLanguageDictionaries(IReadOnlyDictionary{string, IReadOnlyDictionary{string, string}}, bool)"/>
        public void AddLanguageDictionaries(Dictionary<string, Dictionary<string, string>> languages, bool overwriteExisting = true)
        {
            foreach (var (languageName, languageDict) in languages)
            {
                AddLanguage(languageName, languageDict, overwriteExisting);
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

        #region GetTranslationLoader
        /// <summary>
        /// Gets a translation loader with the specified <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// The specified translation loader must have already been added using <see cref="AddTranslationLoader(ITranslationLoader)"/> prior to calling this.
        /// </remarks>
        /// <param name="type">The type of <see cref="ITranslationLoader"/> to get.</param>
        /// <returns><see cref="ITranslationLoader"/> instance with the specified <paramref name="type"/> if found; otherwise, <see langword="null"/>.</returns>
        public ITranslationLoader? GetTranslationLoader(Type type)
        {
            foreach (var loader in TranslationLoaders)
            {
                if (loader.GetType().IsAssignableFrom(type)) return loader;
            }
            return null;
        }
        /// <summary>
        /// Gets a translation loader of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// The specified translation loader must have already been added using <see cref="AddTranslationLoader(ITranslationLoader)"/> prior to calling this.
        /// </remarks>
        /// <typeparam name="T">Class type that implements <see cref="ITranslationLoader"/> to get.</typeparam>
        /// <returns>The <see cref="ITranslationLoader"/> instance if found; otherwise, <see langword="null"/>.</returns>
        public T? GetTranslationLoader<T>() where T : class, ITranslationLoader
            => (T?)GetTranslationLoader(typeof(T))!;
        #endregion GetTranslationLoader

        #region LoadFromString
        /// <summary>
        /// Loads translations from the specified <paramref name="serializedData"/>.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to use for deserializing the string. This must be specified since it can't be automatically determined from a file extension.</param>
        /// <param name="serializedData">A string containing the serialized contents of a translation config file.</param>
        /// <returns><see langword="true"/> when successful; otherwise, <see langword="false"/>.</returns>
        public bool LoadFromString(ITranslationLoader loader, string? serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
                return false;

            var dict = loader.Deserialize(serializedData);
            if (dict == null) return false;
            AddLanguageDictionaries(dict);
            return true;
        }
        #endregion LoadFromString

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
        /// <remarks>
        /// This method catches any exceptions thrown while loading files.
        /// </remarks>
        /// <param name="directoryPath">The path of a directory to load translation configs from.</param>
        /// <param name="recurse">When <see langword="true"/>, translation configs in subdirectories are also loaded.</param>
        /// <returns>An enumerable list of translation config file paths that weren't loaded because none of the available translation loaders support them.</returns>
        /// <exception cref="NoTranslationLoadersException">There weren't any translation loaders in the list.</exception>
        public IEnumerable<string>? LoadFromDirectory(string directoryPath, bool recurse = false)
        {
            if (!Directory.Exists(directoryPath))
                return null;

            var failedFiles = new List<string>();
            foreach (var filePath in Directory.EnumerateFiles(directoryPath, '*' + Loc.ExtensionPrefix + '*', new EnumerationOptions() { RecurseSubdirectories = recurse }))
            {
                try
                {
                    if (!LoadFromFile(filePath))
                    {
                        failedFiles.Add(filePath);
                    }
                }
                catch { }
            }
            return failedFiles;
        }
        #endregion LoadFromDirectory

        #region SaveToString
        /// <summary>
        /// Serializes the translations for all currently loaded languages using the specified <paramref name="loader"/>.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to use for serializing the translations.</param>
        /// <returns>The serialized translations as a <see cref="string"/>.</returns>
        public string SaveToString(ITranslationLoader loader)
        {
            return loader.Serialize(Languages.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<string, string>)kvp.Value));
        }
        /// <summary>
        /// Serializes the specified language using the specified <paramref name="loader"/>.
        /// </summary>
        /// <param name="loader">The <see cref="ITranslationLoader"/> instance to use for serializing the translations.</param>
        /// <param name="languageName">The name of the language to serialize.</param>
        /// <returns>The serialized translations as a <see cref="string"/>.</returns>
        public string? SaveToString(ITranslationLoader loader, string languageName)
        {
            if (!Languages.TryGetValue(languageName, out var langDict))
                return null;

            return loader.Serialize(langDict.ToLanguageDictionaries(languageName));
        }
        #endregion SaveToString

        #region SaveToFile
        /// <summary>
        /// Saves all loaded translations to the specified <paramref name="filePath"/>.
        /// </summary>
        /// <remarks>
        /// The loader is determined based on the specified <paramref name="filePath"/>.<br/>
        /// This method does not catch any exceptions that occur during serialization or write operations.
        /// </remarks>
        /// <param name="filePath">The output filepath.</param>
        /// <param name="useTempFile">When <see langword="true"/>, the translations are saved to a temp file, then moved to the final destination; otherwise, writes directly to the <paramref name="filePath"/>.</param>
        /// <returns><see langword="true"/> when successful; otherwise, <see langword="false"/>.</returns>
        public bool SaveToFile(string filePath, bool useTempFile = true)
        {
            if (GetTranslationLoaderForFile(filePath) is ITranslationLoader loader && SaveToString(loader) is string serializedData)
            {
                if (useTempFile)
                {
                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllText(tempFilePath, serializedData, System.Text.Encoding.UTF8);
                    File.Move(tempFilePath, filePath);
                }
                else File.WriteAllText(filePath, serializedData, System.Text.Encoding.UTF8);
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Saves the translations for the specified <paramref name="languageName"/> to the specified <paramref name="filePath"/>.
        /// </summary>
        /// <remarks>
        /// This method does not catch any exceptions that occur during serialization or write operations.
        /// </remarks>
        /// <param name="filePath">The output filepath.</param>
        /// <param name="languageName">The name of the language to serialize and save to the <paramref name="filePath"/>.</param>
        /// <param name="useTempFile">When <see langword="true"/>, the translations are saved to a temp file, then moved to the final destination; otherwise, writes directly to the <paramref name="filePath"/>.</param>
        /// <returns><see langword="true"/> when successful; otherwise, <see langword="false"/>.</returns>
        public bool SaveToFile(string filePath, string languageName, bool useTempFile = true)
        {
            if (GetTranslationLoaderForFile(filePath) is ITranslationLoader loader && SaveToString(loader, languageName) is string serializedData)
            {
                if (useTempFile)
                {
                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllText(tempFilePath, serializedData, System.Text.Encoding.UTF8);
                    File.Move(tempFilePath, filePath);
                }
                else File.WriteAllText(filePath, serializedData, System.Text.Encoding.UTF8);
                return true;
            }
            else return false;
        }
        #endregion SaveToFile

        #region (Private) WriteFileAsync
        private static async Task<string> WriteFileAsync(string filePath, string content)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var writer = new StreamWriter(File.Open(tempFile, FileMode.Create, FileAccess.Write), System.Text.Encoding.UTF8))
                {
                    await writer.WriteAsync(content);
                    await writer.FlushAsync();
                }

                File.Move(tempFile, filePath);
                return filePath;
            }
            catch
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch { } //< at least we tried
                return string.Empty;
            }
        }
        #endregion (Private) WriteFileAsync

        #region SaveToDirectory
        /// <summary>
        /// Saves all languages to their own UTF8 file in the specified directory using the specified <paramref name="loader"/>. If the directory doesn't exist, it will be created.
        /// </summary>
        /// <remarks>
        /// This method uses asynchronous write operations &amp; catches any exceptions that occur while writing files.
        /// </remarks>
        /// <param name="directoryPath">The path to a directory to save the translations in. If an empty string is provided, the current working directory will be used.</param>
        /// <param name="loader">The <see cref="ITranslationLoader"/> to use for serializing languages.</param>
        /// <param name="fileNameSelector">A selector method that chooses a filename when given a language name.</param>
        /// <returns>A list of the full paths of all successfully-created translation config files.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="directoryPath"/> or <paramref name="loader"/> was <see langword="null"/>.</exception>
        public IEnumerable<string> SaveToDirectory(string directoryPath, ITranslationLoader loader, Func<string, LanguageDictionary, string> fileNameSelector)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));
            else if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            if (directoryPath.Length == 0)
                directoryPath = Environment.CurrentDirectory;
            else
                directoryPath = Path.GetFullPath(directoryPath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var writeTasks = new List<Task<string>>();

            foreach (var (languageName, languageDict) in Languages)
            {
                string path = Path.Combine(directoryPath, Path.GetFileName(fileNameSelector(languageName, languageDict)));

                writeTasks.Add(WriteFileAsync(path, loader.Serialize(languageDict.ToLanguageDictionaries(languageName))));
            }

            Task.WhenAll(writeTasks);

            return writeTasks.Select(t => t.Result);
        }
        #endregion SaveToDirectory

        #region Translate
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found in the current language.</exception>
        public string Translate(string key, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, out string translatedString))
                return translatedString;
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, key);
                // try default text
                if (defaultText != null)
                    return defaultText;
                // try fallback language
                if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, out translatedString))
                    return translatedString;
                // use key / empty string
                return UseKeyAsFallback ? key : string.Empty;
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found in the current language.</exception>
        public string Translate(string key, StringComparison stringComparison, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, stringComparison, out string translatedString))
                return translatedString;
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, key);
                // try default text
                if (defaultText != null)
                    return defaultText;
                // try fallback language
                if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                    return translatedString;
                // use key / empty string
                return UseKeyAsFallback ? key : string.Empty;
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found in the specified language.</exception>
        public string Translate(string key, string? defaultText, string languageName, bool allowFallback = false)
        {
            if (Languages.TryGetValue(languageName, out var dict) && dict.TryGetValue(key, out string translatedString))
                return translatedString;
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, key);
                // try default text
                if (defaultText != null)
                    return defaultText;
                else if (allowFallback)
                { // try current language or fallback language
                    if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, out translatedString))
                        return translatedString;
                    else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, out translatedString))
                        return translatedString;
                }
                // use key / empty string
                return UseKeyAsFallback ? key : string.Empty;
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found in the specified language.</exception>
        public string Translate(string key, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false)
        {
            if (Languages.TryGetValue(languageName, out var dict) && dict.TryGetValue(key, stringComparison, out string translatedString))
                return translatedString;
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, key);
                // try default text
                if (defaultText != null)
                    return defaultText;
                else if (allowFallback)
                { // try current language or fallback language
                    if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                        return translatedString;
                    else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                        return translatedString;
                }
                // use key / empty string
                return UseKeyAsFallback ? key : string.Empty;
            }
        }
        #endregion Translate

        #region TranslateAny
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the current language.
        /// </summary>
        /// <param name="keys">Any number of translation keys in order of priority.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the first existing key; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the first existing <paramref name="keys"/> in the fallback language.<br/>
        /// 3. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and none of the requested <paramref name="keys"/> were found in the current language.</exception>
        public string TranslateAny(IEnumerable<string> keys, string? defaultText = null)
        {
            bool hasKeys = keys.Any();
            if (hasKeys)
            {
                if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetFirstValue(keys, out var translatedString))
                    return translatedString;
                else if (ThrowOnMissingTranslation)
                    throw new MissingTranslationException(CurrentLanguageName, keys);
            }
            // fallback
            NotifyMissingTranslationStringRequested(CurrentLanguageName, keys);
            // try default text
            if (defaultText != null)
                return defaultText;
            // try fallback lang
            else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetFirstValue(keys, out var translatedString))
                return translatedString;
            // use key / empty string
            return UseKeyAsFallback ? string.Join('|', keys) : string.Empty;
        }
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the current language.
        /// </summary>
        /// <param name="keys">Any number of translation keys in order of priority.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the first existing key; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the first existing <paramref name="keys"/> in the fallback language.<br/>
        /// 3. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and none of the requested <paramref name="keys"/> were found in the current language.</exception>
        public string TranslateAny(IEnumerable<string> keys, StringComparison stringComparison, string? defaultText = null)
        {
            bool hasKeys = keys.Any();
            if (hasKeys)
            {
                if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetFirstValue(keys, stringComparison, out var translatedString))
                    return translatedString;
                else if (ThrowOnMissingTranslation)
                    throw new MissingTranslationException(CurrentLanguageName, keys);
            }
            // fallback
            NotifyMissingTranslationStringRequested(CurrentLanguageName, keys);
            // try default text
            if (defaultText != null)
                return defaultText;
            // try fallback lang
            else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetFirstValue(keys, stringComparison, out var translatedString))
                return translatedString;
            // use key / empty string
            return UseKeyAsFallback ? string.Join('|', keys) : string.Empty;
        }
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the specified language.
        /// </summary>
        /// <param name="keys">Any number of translation keys in order of priority.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the first existing key; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the current language.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the fallback language.<br/>
        /// 4. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and none of the requested <paramref name="keys"/> were found in the specified language.</exception>
        public string TranslateAny(IEnumerable<string> keys, string? defaultText, string languageName, bool allowFallback = false)
        {
            bool hasKeys = keys.Any();
            if (hasKeys)
            {
                if (Languages.TryGetValue(languageName, out var langDict) && langDict.TryGetFirstValue(keys, out var translatedString))
                    return translatedString;
                else if (ThrowOnMissingTranslation)
                    throw new MissingTranslationException(languageName, keys);
            }
            // fallback
            NotifyMissingTranslationStringRequested(languageName, keys);
            // try default text
            if (defaultText != null)
                return defaultText;
            // try current/fallback lang
            else if (hasKeys && allowFallback)
            {
                if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetFirstValue(keys, out var translatedString))
                    return translatedString;
                else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetFirstValue(keys, out translatedString))
                    return translatedString;
            }
            // use key / empty string
            return UseKeyAsFallback ? string.Join('|', keys) : string.Empty;
        }
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the specified language.
        /// </summary>
        /// <param name="keys">Any number of translation keys in order of priority.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the first existing key; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the current language.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the fallback language.<br/>
        /// 4. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and none of the requested <paramref name="keys"/> were found in the specified language.</exception>
        public string TranslateAny(IEnumerable<string> keys, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false)
        {
            bool hasKeys = keys.Any();
            if (hasKeys)
            { // search for the first existing key
                if (Languages.TryGetValue(languageName, stringComparison, out var langDict) && langDict.TryGetFirstValue(keys, stringComparison, out var translatedString))
                    return translatedString;
                else if (ThrowOnMissingTranslation)
                    throw new MissingTranslationException(languageName, keys);
            }
            // fallback
            NotifyMissingTranslationStringRequested(languageName, keys);
            // try default text
            if (defaultText != null)
                return defaultText;
            // try current/fallback lang
            else if (hasKeys && allowFallback)
            {
                if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetFirstValue(keys, stringComparison, out var translatedString))
                    return translatedString;
                else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetFirstValue(keys, stringComparison, out translatedString))
                    return translatedString;
            }
            // use key / empty string
            return UseKeyAsFallback ? string.Join('|', keys) : string.Empty;
        }
        #endregion TranslateAny

        #region TranslateAll
        /// <summary>
        /// Gets the translations for all of the specified <paramref name="keys"/> in the current language.
        /// </summary>
        /// <param name="keys">Any number of translation keys. The output will be the same length and order.</param>
        /// <param name="defaultText">A string to return when a requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translations for each key when found; otherwise, translations come from one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the first existing <paramref name="keys"/> in the fallback language.<br/>
        /// 3. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and at least one of the requested <paramref name="keys"/> wasn't found in the current language.</exception>
        public IEnumerable<string> TranslateAll(IEnumerable<string> keys, string? defaultText = null)
            => keys.Select(k => Translate(k, defaultText));
        /// <summary>
        /// Gets the translations for all of the specified <paramref name="keys"/> in the current language.
        /// </summary>
        /// <param name="keys">Any number of translation keys. The output will be the same length and order.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when a requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translations for each key when found; otherwise, translations come from one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the first existing <paramref name="keys"/> in the fallback language.<br/>
        /// 3. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 4. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and at least one of the requested <paramref name="keys"/> wasn't found in the current language.</exception>
        public IEnumerable<string> TranslateAll(IEnumerable<string> keys, StringComparison stringComparison, string? defaultText = null)
            => keys.Select(k => Translate(k, stringComparison, defaultText));
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the specified language.
        /// </summary>
        /// <param name="keys">Any number of translation keys. The output will be the same length and order.</param>
        /// <param name="defaultText">A string to return when a requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get translations for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translations for each key when found; otherwise, translations come from one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the current language.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the fallback language.<br/>
        /// 4. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and at least one of the requested <paramref name="keys"/> wasn't found in the specified language.</exception>
        public IEnumerable<string> TranslateAll(IEnumerable<string> keys, string? defaultText, string languageName, bool allowFallback = false)
            => keys.Select(k => Translate(k, defaultText, languageName, allowFallback));
        /// <summary>
        /// Gets the translation for any of the specified <paramref name="keys"/> in the specified language.
        /// </summary>
        /// <param name="keys">Any number of translation keys. The output will be the same length and order.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when a requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get translations for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translations for each key when found; otherwise, translations come from one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the current language.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the first existing key in the fallback language.<br/>
        /// 4. Concatenated <paramref name="keys"/> when UseKeyAsFallback is <see langword="true"/>. The keys are separated with a vertical bar '|' character.<br/>
        /// 5. An empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and at least one of the requested <paramref name="keys"/> wasn't found in the specified language.</exception>
        public IEnumerable<string> TranslateAll(IEnumerable<string> keys, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false)
            => keys.Select(k => Translate(k, stringComparison, defaultText, languageName, allowFallback));
        #endregion TranslateAll

        #region (Static) Tr
        /// <summary>
        /// Uses the default Instance to get the translation for the specified <paramref name="key"/> in the current language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string key, string? defaultText = null) => Instance.Translate(key, defaultText);
        /// <summary>
        /// Uses the default Instance to get the translation for the specified <paramref name="key"/> in the current language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. An empty string.
        /// </returns>
        public static string Tr(string key, StringComparison stringComparison, string? defaultText = null) => Instance.Translate(key, stringComparison, defaultText);
        /// <summary>
        /// Uses the default Instance to get the translation for the specified <paramref name="key"/> in the specified language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. An empty string.
        /// </returns>
        public static string Tr(string key, string? defaultText, string languageName, bool allowFallback = false) => Instance.Translate(key, defaultText, languageName, allowFallback);
        /// <summary>
        /// Uses the default Instance to get the translation for the specified <paramref name="key"/> in the specified language.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// The translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. The <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> The translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. The <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. An empty string.
        /// </returns>
        public static string Tr(string key, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false) => Instance.Translate(key, stringComparison, defaultText, languageName, allowFallback);
        #endregion (Static) Tr

        #region TranslateWithContext
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public TranslationContext TranslateWithContext(string key, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, out string translatedString))
                return new TranslationContext(key, translatedString, CurrentLanguageName, TranslationContext.TranslationSource.CurrentLanguage);
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, key);
                // try default text
                if (defaultText != null)
                    return new TranslationContext(key, defaultText, TranslationContext.TranslationSource.DefaultText);
                // try fallback language
                if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, out translatedString))
                    return new TranslationContext(key, translatedString, FallbackLanguageName, TranslationContext.TranslationSource.FallbackLanguage);
                // try string path
                if (UseKeyAsFallback)
                    return new TranslationContext(key, key, TranslationContext.TranslationSource.Key);
                // use empty string
                return new TranslationContext(key, string.Empty, TranslationContext.TranslationSource.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public TranslationContext TranslateWithContext(string key, StringComparison stringComparison, string? defaultText = null)
        {
            if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, stringComparison, out string translatedString))
                return new TranslationContext(key, translatedString, CurrentLanguageName, TranslationContext.TranslationSource.CurrentLanguage);
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(CurrentLanguageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(CurrentLanguageName, key);
                // try default text
                if (defaultText != null)
                    return new TranslationContext(key, defaultText, TranslationContext.TranslationSource.DefaultText);
                // try fallback language
                if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                    return new TranslationContext(key, translatedString, FallbackLanguageName, TranslationContext.TranslationSource.FallbackLanguage);
                // try string path
                if (UseKeyAsFallback)
                    return new TranslationContext(key, key, TranslationContext.TranslationSource.Key);
                // use empty string
                return new TranslationContext(key, string.Empty, TranslationContext.TranslationSource.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public TranslationContext TranslateWithContext(string key, string? defaultText, string languageName, bool allowFallback = false)
        {
            if (Languages.TryGetValue(languageName, out var dict) && dict.TryGetValue(key, out string translatedString))
                return new TranslationContext(key, translatedString, languageName, TranslationContext.TranslationSource.ExplicitLanguage);
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, key);
                // try default text
                if (defaultText != null)
                    return new TranslationContext(key, defaultText, TranslationContext.TranslationSource.DefaultText);
                else if (allowFallback)
                { // try current language or fallback language
                    if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, out translatedString))
                        return new TranslationContext(key, translatedString, TranslationContext.TranslationSource.CurrentLanguage);
                    else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, out translatedString))
                        return new TranslationContext(key, translatedString, TranslationContext.TranslationSource.FallbackLanguage);
                }
                // try string path
                if (UseKeyAsFallback)
                    return new TranslationContext(key, key, TranslationContext.TranslationSource.Key);
                // use empty string
                return new TranslationContext(key, string.Empty, TranslationContext.TranslationSource.Empty);
            }
        }
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public TranslationContext TranslateWithContext(string key, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false)
        {
            if (Languages.TryGetValue(languageName, out var dict) && dict.TryGetValue(key, stringComparison, out string translatedString))
                return new TranslationContext(key, translatedString, languageName, TranslationContext.TranslationSource.ExplicitLanguage);
            else if (ThrowOnMissingTranslation)
                throw new MissingTranslationException(languageName, key);
            else
            {
                NotifyMissingTranslationStringRequested(languageName, key);
                // try default text
                if (defaultText != null)
                    return new TranslationContext(key, defaultText, TranslationContext.TranslationSource.DefaultText);
                else if (allowFallback)
                { // try current language or fallback language
                    if (CurrentLanguageDictionary != null && CurrentLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                        return new TranslationContext(key, translatedString, TranslationContext.TranslationSource.CurrentLanguage);
                    else if (FallbackLanguageDictionary != null && FallbackLanguageDictionary.TryGetValue(key, stringComparison, out translatedString))
                        return new TranslationContext(key, translatedString, TranslationContext.TranslationSource.FallbackLanguage);
                }
                // try string path
                if (UseKeyAsFallback)
                    return new TranslationContext(key, key, TranslationContext.TranslationSource.Key);
                // use empty string
                return new TranslationContext(key, string.Empty, TranslationContext.TranslationSource.Empty);
            }
        }
        #endregion TranslateWithContext

        #region (Static) ContextTr
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public static TranslationContext ContextTr(string key, string? defaultText = null) => Instance.TranslateWithContext(key, defaultText);
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the current language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the current language.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. The wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 3. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 4. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public static TranslationContext ContextTr(string key, StringComparison stringComparison, string? defaultText = null) => Instance.TranslateWithContext(key, stringComparison, defaultText);
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public static TranslationContext ContextTr(string key, string? defaultText, string languageName, bool allowFallback = false) => Instance.TranslateWithContext(key, defaultText, languageName, allowFallback);
        /// <summary>
        /// Gets the translation for the specified <paramref name="key"/> in the specified language with a <see cref="TranslationContext"/> wrapper.
        /// </summary>
        /// <param name="key">The path of the target translated string.</param>
        /// <param name="stringComparison">The <see cref="StringComparison"/> type to use for comparing strings.</param>
        /// <param name="defaultText">A string to return when the requested translation wasn't found for the specified language.</param>
        /// <param name="languageName">The name of the language to get the translation for.</param>
        /// <param name="allowFallback">When <see langword="true"/>, the translation from the current language or fallback language may be used when not found in the specified <paramref name="languageName"/>; otherwise when <see langword="false"/>, only the translation from the specified language may be used.</param>
        /// <returns>
        /// A wrapped translation for the specified <paramref name="key"/> when found; otherwise, one of the fallback sources (in order):<br/>
        /// 1. Wrapped <paramref name="defaultText"/> if it isn't <see langword="null"/>.<br/>
        /// 2. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the current language if found.<br/>
        /// 3. <i>(Only when <paramref name="allowFallback"/> is <see langword="true"/>)</i> Wrapped translation for the specified <paramref name="key"/> in the fallback language if found.<br/>
        /// 4. Wrapped <paramref name="key"/> when UseKeyAsFallback is <see langword="true"/>.<br/>
        /// 5. Wrapped empty string.
        /// </returns>
        /// <exception cref="MissingTranslationException">ThrowOnMissingTranslation was <see langword="true"/> and the requested <paramref name="key"/> wasn't found.</exception>
        public static TranslationContext ContextTr(string key, StringComparison stringComparison, string? defaultText, string languageName, bool allowFallback = false) => Instance.TranslateWithContext(key, stringComparison, defaultText, languageName, allowFallback);
        #endregion (Static) TranslateWithContext

        #endregion Methods
    }
}
