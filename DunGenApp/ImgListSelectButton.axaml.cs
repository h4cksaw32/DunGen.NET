using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Media;

namespace DunGenApp;

public partial class ImgListSelectButton : UserControl
{
    public int Index { get; set; }
    public Image?[] Collection { get; set; }
    public Image? Image {
        get => Collection[Index];
        set => Collection[Index] = value;
    }
    public ImgListSelectButton(int i, Image?[] list)
    {
        Index = i;
        Collection = list;
        if (Image == null) Image = new Image { Source = new Bitmap("default.png"), Stretch = Stretch.None };
        InitializeComponent();
        if (Image != null) Disp.Content = Image;
    }
    private async void SelectImage(object source, RoutedEventArgs ev)
    {
        var files = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select Image",
            AllowMultiple = false
        });
        if (files != null && files.Count >= 1)
        {
            if (Image == null) Image = new Image();
            Image.Source = new Bitmap(await files[0].OpenReadAsync());
            Image.Stretch = Stretch.None;
            Disp.Content = Image;
        }
    }
}