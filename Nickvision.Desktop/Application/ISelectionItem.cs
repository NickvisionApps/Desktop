using System.ComponentModel;

namespace Nickvision.Desktop.Application;

public interface ISelectionItem : INotifyPropertyChanged
{
    string Label { get; }
    bool ShouldSelect { get; }
}
