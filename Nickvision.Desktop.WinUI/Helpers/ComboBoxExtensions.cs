using Nickvision.Desktop.Application;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class ComboBoxExtensions
{
    extension(ComboBox comboBox)
    {
        public void SelectSelectionItem()
        {
            if (comboBox.ItemsSource is IReadOnlyList<ISelectionItem> items)
            {
                comboBox.SelectedItem = items.FirstOrDefault(item => item.ShouldSelect);
            }
        }
    }
}
