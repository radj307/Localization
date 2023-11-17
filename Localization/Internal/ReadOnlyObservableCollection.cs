using System.Collections.ObjectModel;

namespace Localization.Internal
{
    internal class ReadOnlyObservableCollection<T> : ObservableCollection<T>, IReadOnlyObservableCollection<T> { }
}
