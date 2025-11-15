using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace Nickvision.Desktop.WinUI.Controls;

public sealed partial class ViewStack : UserControl
{
    public static readonly DependencyProperty PagesProperty = DependencyProperty.Register(nameof(Pages), typeof(ObservableCollection<UIElement>), typeof(ViewStack), new PropertyMetadata(new ObservableCollection<UIElement>()));
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ViewStack), new PropertyMetadata(0));

    public ViewStack()
    {
        InitializeComponent();
    }

    public ObservableCollection<UIElement> Pages
    {
        get => (ObservableCollection<UIElement>)GetValue(PagesProperty);

        set
        {
            SetValue(PagesProperty, value);
            if(value.Count > 0)
            {
                Content = Pages[SelectedIndex];
            }
        }
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);

        set
        {
            SetValue(SelectedIndexProperty, value);
            if (Pages.Count > 0 && value >= 0 && value < Pages.Count)
            {
                Content = Pages[value];
            }
        }
    }
}
