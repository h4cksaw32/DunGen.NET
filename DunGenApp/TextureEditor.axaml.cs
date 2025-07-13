using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;

namespace DunGenApp;

public partial class TextureEditor : Window
{
    private TextureOptions textures;
    public TextureEditor(TextureOptions t)
    {
        textures = t;
        InitializeComponent();
    }
}
public class IndexedButton : Button
{
    int Index { get; set; }
}
public class TextureOptions
{
    private readonly Image?[] textures = new Image?[256];
    private readonly Image[] player = new Image[8];
    private ushort tileSize = 32;
    private float scale = 1;
    public Image?[] Textures => textures;
    public Image[] Player => player;
    public ushort TileSize { get => tileSize; set => tileSize = value; }
    public float Scale { get => scale; set => scale = value; }
}