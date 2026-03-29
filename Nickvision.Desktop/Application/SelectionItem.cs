using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nickvision.Desktop.Application;

public abstract class SelectionItem : INotifyPropertyChanged
{
    public string Label { get; init; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SelectionItem(string label, bool shouldSelect)
    {
        Label = label;
        ShouldSelect = shouldSelect;
    }

    public bool ShouldSelect
    {
        get => field;

        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class SelectionItem<T> : SelectionItem
{
    public T Value { get; }

    public SelectionItem(T value, string label, bool shouldSelect) : base(label, shouldSelect)
    {
        Value = value;
    }
}