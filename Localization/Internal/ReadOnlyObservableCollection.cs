using System.Collections.ObjectModel;

namespace Localization.Internal
{
    public class ReadOnlyObservableCollection<T> : ObservableCollection<T>, IReadOnlyObservableCollection<T> { }
}
