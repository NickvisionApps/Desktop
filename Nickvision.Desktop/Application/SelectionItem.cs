using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nickvision.Desktop.Application;

public class SelectionItem<T> : ISelectionItem
{
    public T Value { get; }
    public string Label { get; }
    public bool ShouldSelect { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SelectionItem(T value, string label, bool shouldSelect)
    {
        Value = value;
        Label = label;
        ShouldSelect = shouldSelect;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}