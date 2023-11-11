using System.Collections.Generic;
using System.Collections.Specialized;

namespace Localization.Internal
{
    public interface IReadOnlyObservableCollection<T> : IReadOnlyCollection<T>, ICollection<T>, INotifyCollectionChanged { }
}
