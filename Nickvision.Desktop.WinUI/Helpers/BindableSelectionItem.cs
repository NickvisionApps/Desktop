using Nickvision.Desktop.Application;
using WinRT;

namespace Nickvision.Desktop.WinUI.Helpers;

[GeneratedBindableCustomProperty]
public sealed partial class BindableSelectionItem
{
    private readonly ISelectionItem _selectionItem;

    public BindableSelectionItem(ISelectionItem selectionItem)
    {
        _selectionItem = selectionItem;
    }

    public string Label => _selectionItem.Label;

    public bool ShouldSelect
    {
        get => _selectionItem.ShouldSelect;

        set => _selectionItem.ShouldSelect = value;
    }

    public static implicit operator BindableSelectionItem(SelectionItem<object> item) => new BindableSelectionItem(item);
}
