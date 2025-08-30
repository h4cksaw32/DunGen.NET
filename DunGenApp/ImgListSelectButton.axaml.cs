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
    public Bitmap?[] Collection { get; set; }
    public Bitmap? Bitmap {
        get => Collection[Index];
        set => Collection[Index] = value;
    }
    public ImgListSelectButton(int i, Bitmap?[] list)
    {
        Index = i;
        Collection = list;
        if (Bitmap == null) Bitmap = new Bitmap("default.png");
        InitializeComponent();
        if (Bitmap != null) Disp.Content = new Image { Source = Bitmap, Stretch = Stretch.None };
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
            Bitmap = new Bitmap(await files[0].OpenReadAsync());
            Disp.Content = new Image { Source = Bitmap, Stretch = Stretch.None };
        }
    }
}