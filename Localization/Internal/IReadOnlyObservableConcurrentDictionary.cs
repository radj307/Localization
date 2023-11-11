//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ObservableConcurrentDictionary.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Localization.Internal
{
    public interface IReadOnlyObservableConcurrentDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged { }
}
