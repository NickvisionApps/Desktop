using Nickvision.Desktop.Application;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class ListViewExtensions
{
    extension(ListView listView)
    {
        public void SelectSelectionItems()
        {
            if (listView.ItemsSource is IReadOnlyList<ISelectionItem> items)
            {
                foreach (var item in items.Where(i => i.ShouldSelect))
                {
                    listView.SelectedItems.Add(item);
                }
            }
        }
    }
}
