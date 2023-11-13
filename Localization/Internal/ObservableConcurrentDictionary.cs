﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ObservableConcurrentDictionary.cs
//
//--------------------------------------------------------------------------

using PropertyChanged;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Localization.Internal
{
    /// <summary>
    /// Provides a thread-safe dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class ObservableConcurrentDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        IReadOnlyObservableConcurrentDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext _context;
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ObservableConcurrentDictionary()
        {
            _context = AsyncOperationManager.SynchronizationContext;
            _dictionary = new ConcurrentDictionary<TKey, TValue>();
        }
        /// <summary>
        /// Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            _context = AsyncOperationManager.SynchronizationContext;
            _dictionary = new ConcurrentDictionary<TKey, TValue>(other);
        }

        /// <summary>Event raised when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>Event raised when a property on the collection changes.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
        /// </summary>
        private void NotifyObserversOfChange()
        {
            var collectionHandler = CollectionChanged;
            var propertyHandler = PropertyChanged;
            if (collectionHandler != null || propertyHandler != null)
            {
                _context.Post(s =>
                {
                    collectionHandler?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    if (propertyHandler != null)
                    {
                        propertyHandler(this, new PropertyChangedEventArgs("Count"));
                        propertyHandler(this, new PropertyChangedEventArgs("Keys"));
                        propertyHandler(this, new PropertyChangedEventArgs("Values"));
                    }
                }, null);
            }
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return TryAddWithNotification(item.Key, item.Value);
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be added.</param>
        /// <param name="value">The value of the item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(TKey key, TValue value)
        {
            bool result = _dictionary.TryAdd(key, value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        /// <summary>Attempts to remove an item from the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be removed.</param>
        /// <param name="value">The value of the item removed.</param>
        /// <returns>Whether the removal was successful.</returns>
        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            bool result = _dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        /// <summary>Attempts to add or update an item in the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be updated.</param>
        /// <param name="value">The new value to set for the item.</param>
        /// <returns>Whether the update was successful.</returns>
        private void UpdateWithNotification(TKey key, TValue value)
        {
            _dictionary[key] = value;
            NotifyObserversOfChange();
        }

        #region ICollection<KeyValuePair<TKey,TValue>> Members
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            TryAddWithNotification(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Clear();
            NotifyObserversOfChange();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemoveWithNotification(item.Key, out _);
        }
        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();
        }
        #endregion

        #region IDictionary<TKey,TValue> Members
        public void Add(TKey key, TValue value)
        {
            TryAddWithNotification(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            return TryRemoveWithNotification(key, out _);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)_dictionary).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)_dictionary).Values;

        public int Count => ((IReadOnlyCollection<KeyValuePair<TKey, TValue>>)_dictionary).Count;

        [SuppressPropertyChangedWarnings]
        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set { UpdateWithNotification(key, value); }
        }
        #endregion
    }
}
