using Nickvision.Desktop.Application;
using System.ComponentModel;
using WinRT;

namespace Nickvision.Desktop.WinUI.Helpers;

[GeneratedBindableCustomProperty]
public sealed partial class BindableSelectionItem : INotifyPropertyChanged
{
    private readonly SelectionItem _selectionItem;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableSelectionItem(SelectionItem selectionItem)
    {
        _selectionItem = selectionItem;
        _selectionItem.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
    }

    public string Label => _selectionItem.Label;

    public bool ShouldSelect
    {
        get => _selectionItem.ShouldSelect;

        set => _selectionItem.ShouldSelect = value;
    }

    public SelectionItem<T>? ToSelectionItem<T>()
    {
        if (_selectionItem is SelectionItem<T> source)
        {
            return source;
        }
        return null;
    }
}
