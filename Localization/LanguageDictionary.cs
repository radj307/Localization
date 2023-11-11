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
    public sealed class LanguageDictionary : IDictionary<string, string>, IReadOnlyDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors
        public LanguageDictionary() : this(new ObservableConcurrentDictionary<string, string>()) { }
        public LanguageDictionary(ObservableConcurrentDictionary<string, string> translations)
        {
            _translationStrings = translations;
        }
        public LanguageDictionary(IReadOnlyDictionary<string, string> translatedStrings)
        {
            _translationStrings = new ObservableConcurrentDictionary<string, string>();
            Merge(translatedStrings);
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
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AsLanguage(string languageName)
        {
            return new Dictionary<string, IReadOnlyDictionary<string, string>>()
            {
                { languageName, this }
            };
        }
        #endregion Methods

        #region Interface Implementation

        #region Properties
        public ICollection<string> Keys => ((IDictionary<string, string>)_translationStrings).Keys;
        public ICollection<string> Values => ((IDictionary<string, string>)_translationStrings).Values;
        public int Count => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_translationStrings).IsReadOnly;
        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => ((IReadOnlyDictionary<string, string>)_translationStrings).Keys;
        IEnumerable<string> IReadOnlyDictionary<string, string>.Values => ((IReadOnlyDictionary<string, string>)_translationStrings).Values;
        #endregion Properties

        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => ((INotifyCollectionChanged)_translationStrings).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)_translationStrings).CollectionChanged -= value;
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => ((INotifyPropertyChanged)_translationStrings).PropertyChanged += value;
            remove => ((INotifyPropertyChanged)_translationStrings).PropertyChanged -= value;
        }
        #endregion Events

        #region Methods
        public void Add(string key, string value) => ((IDictionary<string, string>)_translationStrings).Add(key, value);
        public bool ContainsKey(string key) => ((IDictionary<string, string>)_translationStrings).ContainsKey(key);
        public bool Remove(string key) => ((IDictionary<string, string>)_translationStrings).Remove(key);
        public bool TryGetValue(string key, out string value) => ((IDictionary<string, string>)_translationStrings).TryGetValue(key, out value);
        public void Add(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Add(item);
        public void Clear() => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Clear();
        public bool Contains(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Contains(item);
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_translationStrings).Remove(item);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)_translationStrings).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_translationStrings).GetEnumerator();
        #endregion Methods

        #endregion Interface Implementation
    }
}
