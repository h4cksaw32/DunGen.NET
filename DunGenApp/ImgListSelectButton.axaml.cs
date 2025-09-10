using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace DunGenApp;

public partial class ImgListSelectButton : UserControl
{
    public int Index { get; set; }
    public Bitmap?[] Collection { get; set; }
    public Bitmap? Bitmap {
        get => Collection[Index];
        set => Collection[Index] = value;
    }
    public ImgListSelectButton(int i, Bitmap?[] list,  Bitmap defImage, int sizeLimit = 100)
    {
        Index = i;
        Collection = list;
        if (Bitmap == null) Bitmap = defImage;
        MaxWidth = sizeLimit;
        MaxHeight = sizeLimit;
        InitializeComponent();
        if (Bitmap != null) Disp.Content = new Image { Source = Bitmap, Stretch = Stretch.None };
    }
    private async void SelectImage(object source, RoutedEventArgs ev)
    {
        var files = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });
        if (files != null && files.Count >= 1)
        {
            Bitmap = new Bitmap(await files[0].OpenReadAsync());
            Disp.Content = new Image { Source = Bitmap, Stretch = Stretch.None };
        }
    }
}