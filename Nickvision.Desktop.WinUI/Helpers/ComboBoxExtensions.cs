using Microsoft.UI.Xaml.Controls;
using Nickvision.Desktop.Application;
using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class ComboBoxExtensions
{
    extension(ComboBox comboBox)
    {
        public void SelectSelectionItem()
        {
            if (comboBox.ItemsSource is IEnumerable<SelectionItem> items)
            {
                comboBox.SelectedItem = items.FirstOrDefault(item => item.ShouldSelect);
            }
            else if (comboBox.ItemsSource is IEnumerable<BindableSelectionItem> bindableItems)
            {
                comboBox.SelectedItem = bindableItems.FirstOrDefault(item => item.ShouldSelect);
            }
        }
    }
}
