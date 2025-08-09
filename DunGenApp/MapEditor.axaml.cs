using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DunGenLib;

namespace DunGenApp;

public partial class MapEditor : Window
{
    public Map map;
    public TextureOptions textures;
    public MapEditor(Map m, TextureOptions t)
    {
        map = m;
        textures = t;
        InitializeComponent();
    }
    private void FillMap(object? source, RoutedEventArgs e)
    {
        textures.gen.FillMap();
    }
}