using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace Nickvision.Desktop.WinUI.Controls;

public sealed partial class ViewStack : UserControl
{
    public static readonly DependencyProperty PagesProperty = DependencyProperty.Register(nameof(Pages), typeof(ObservableCollection<UIElement>), typeof(ViewStack), new PropertyMetadata(null));
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ViewStack), new PropertyMetadata(0));

    public int PreviousSelectedIndex { get; private set; }

    public ViewStack()
    {
        InitializeComponent();
        Pages = new ObservableCollection<UIElement>();
        PreviousSelectedIndex = 0;
    }

    public ObservableCollection<UIElement> Pages
    {
        get => (ObservableCollection<UIElement>)GetValue(PagesProperty);

        set
        {
            SetValue(PagesProperty, value);
            if (value.Count > 0)
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
            PreviousSelectedIndex = SelectedIndex;
            SetValue(SelectedIndexProperty, value);
            if (Pages.Count > 0 && value >= 0 && value < Pages.Count)
            {
                Content = Pages[value];
            }
        }
    }
}
