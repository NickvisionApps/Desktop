using Nickvision.Desktop.Application;
using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class SelectionItemListExtensions
{
    extension(IEnumerable<SelectionItem> items)
    {
        public List<BindableSelectionItem> ToBindableSelectonItems() => items.Select(i => new BindableSelectionItem(i)).ToList();
    }
}
