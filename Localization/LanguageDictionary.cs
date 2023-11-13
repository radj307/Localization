using Localization.Internal;
using PropertyChanged;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Localization
{
    /// <summary>
    /// A dictionary that contains the translation strings for a single language.
    /// </summary>
    /// <remarks>
    /// Each key contains the path to the corresponding value, and values contain the translated string.
    /// </remarks>
    [DoNotNotify]
    public sealed class LanguageDictionary :
        ICollection<KeyValuePair<string, string>>, IDictionary<string, string>,
        IReadOnlyObservableConcurrentDictionary<string, string>,
        IReadOnlyDictionary<string, string>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors
        /// <summary>
        /// Creates a new empty <see cref="LanguageDictionary"/> instance.
        /// </summary>
        public LanguageDictionary() : this(new ObservableConcurrentDictionary<string, string>()) { }
        /// <summary>
        /// Creates a new <see cref="LanguageDictionary"/> instance with the specified <paramref name="translations"/>.
        /// </summary>
        /// <param name="translations">A dictionary containing translations.</param>
        public LanguageDictionary(ObservableConcurrentDictionary<string, string> translations)
        {
            _translationStrings = translations;
        }
        /// <summary>
        /// Creates a new <see cref="LanguageDictionary"/> instance with the specified <paramref name="translations"/>.
        /// </summary>
        /// <param name="translations">A dictionary containing translations.</param>
        public LanguageDictionary(IReadOnlyDictionary<string, string> translations)
        {
            _translationStrings = new ObservableConcurrentDictionary<string, string>();
            Merge(translations);
        }
        #endregion Constructors

        #region Fields
        private readonly ObservableConcurrentDictionary<string, string> _translationStrings;
        #endregion Fields

        #region Indexers
        /// <summary>
        /// Gets or sets the translated string at the specified <paramref name="stringPath"/>.
        /// </summary>
        /// <inheritdoc/>
        public string this[string stringPath]
        {
            get
            {
                if (_translationStrings.TryGetValue(stringPath, out var value))
                    return value;
                else
                    return null!;
            }
            set => _translationStrings[stringPath] = value;
        }
        #endregion Indexers

        #region Methods
        /// <summary>
        /// Merges the specified <paramref name="otherDictionary"/> into this one.
        /// </summary>
        /// <param name="otherDictionary">Another dictionary instance to merge into this one.</param>
        /// <param name="overwriteExisting">When <see langword="true"/>, existing translation strings are overwritten by the ones from the <paramref name="otherDictionary"/>.</param>
        public void Merge(IReadOnlyDictionary<string, string> otherDictionary, bool overwriteExisting = true)
        {
            foreach (var (path, value) in otherDictionary)
            {
                if (!this.TryAdd(path, value) && overwriteExisting)
                    this[path] = value;
            }
        }
        /// <summary>
        /// Creates a new language dictionary from this instance and the specified <paramref name="languageName"/>.
        /// </summary>
        /// <param name="languageName">The name of this language.</param>
        /// <returns>A dictionary where the keys correspond to the language name, and values are subdictionaries where the keys are the paths and values are the translated strings.</returns>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ToLanguageDictionaries(string languageName)
        {
            return new Dictionary<string, IReadOnlyDictionary<string, string>>()
            {
                { languageName, this }
            };
        }
        #endregion Methods

        #region Interface Implementation

        #region Properties
        /// <inheritdoc/>
        public ICollection<string> Keys => ((IDictionary<string, string>)_translationStrings).Keys;
        /// <inheritdoc/>
        public ICollection<string> Values => ((IDictionary<string, string>)_translationStrings).Values;
        /// <inheritdoc/>
        public int Count => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Count;
        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_translationStrings).IsReadOnly;
        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => ((IReadOnlyDictionary<string, string>)_translationStrings).Keys;
        IEnumerable<string> IReadOnlyDictionary<string, string>.Values => ((IReadOnlyDictionary<string, string>)_translationStrings).Values;
        #endregion Properties

        #region Events
        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => ((INotifyCollectionChanged)_translationStrings).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)_translationStrings).CollectionChanged -= value;
        }
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => ((INotifyPropertyChanged)_translationStrings).PropertyChanged += value;
            remove => ((INotifyPropertyChanged)_translationStrings).PropertyChanged -= value;
        }
        #endregion Events

        #region Methods
        /// <inheritdoc/>
        public void Add(string key, string value) => ((IDictionary<string, string>)_translationStrings).Add(key, value);
        /// <inheritdoc/>
        public bool ContainsKey(string key) => ((IDictionary<string, string>)_translationStrings).ContainsKey(key);
        /// <inheritdoc/>
        public bool Remove(string key) => ((IDictionary<string, string>)_translationStrings).Remove(key);
        /// <inheritdoc/>
        public bool TryGetValue(string key, out string value) => ((IDictionary<string, string>)_translationStrings).TryGetValue(key, out value);
        /// <inheritdoc/>
        public void Add(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Add(item);
        /// <inheritdoc/>
        public void Clear() => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Clear();
        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Contains(item);
        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Remove(item);
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)_translationStrings).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_translationStrings).GetEnumerator();
        #endregion Methods

        #endregion Interface Implementation
    }
}
