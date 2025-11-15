using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;

namespace Nickvision.Desktop.WinUI.Controls;

public sealed partial class StatusPage : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(StatusPage), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(StatusPage), new PropertyMetadata(null));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(StatusPage), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(StatusPage), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(StatusPage), new PropertyMetadata(null));
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register("IsCompact", typeof(bool), typeof(StatusPage), new PropertyMetadata(false));

    public StatusPage()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);

        set
        {
            SetValue(GlyphProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Glyph)));
        }
    }

    public ImageSource? Image
    {
        get => (ImageSource?)GetValue(ImageProperty);

        set
        {
            SetValue(ImageProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
        }
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        
        set
        {
            SetValue(TitleProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);

        set
        {
            SetValue(DescriptionProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        }
    }

    public UIElement? Child
    {
        get => (UIElement?)GetValue(ChildProperty);

        set
        {
            SetValue(ChildProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
        }
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);

        set
        {
            SetValue(IsCompactProperty, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompact)));
        }
    }
}
