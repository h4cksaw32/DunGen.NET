using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DunGenLib;
using System;
using System.Drawing;

namespace DunGenApp;

public partial class MapEditor : Window
{
    public Map map;
    public TextureOptions textures;
    private byte dispSize = 32;
    private ushort xPos = 0;
    private ushort yPos = 0;
    public byte DispSize { get => dispSize; set => dispSize = value; }
    public MapEditor(Map m, TextureOptions t)
    {
        map = m;
        textures = t;
        MenuItem mi = new MenuItem();
        InitializeComponent();
    }
    private void FillMap(object? source, RoutedEventArgs e)
    {
        textures.gen.FillMap();
    }
    private void ResizeDisp(object? source, AvaloniaPropertyChangedEventArgs e)
    {

    }
    private void UpdateSelectIcon(object? source, RoutedEventArgs e)
    {

    }
    private Bitmap VisualizeMap()
    {
        Bitmap bmp = new(map.width, map.height);
        byte b;
        int i;
        Color wallCol = Color.FromArgb(191, 127, 0);
        for (ushort y = 0; y < map.height; y++)
        {
            for (ushort x = 0; x < map.width; x++)
            {
                b = map.GetTile(x, y);
                i = textures.gen.InGroundIDs(b);
                if (i > -1)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(0, (i + 1) / textures.gen.GroundIDs.Count * 255, 0));
                    continue;
                }
                i = textures.gen.InPoolIDs(b);
                if (i > -1)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(0, (i + 1) / textures.gen.PoolIDs.Count * 255, 0));
                    continue;
                }
                if (b == textures.gen.WallID) bmp.SetPixel(x, y, wallCol);
            }
        }
        return bmp;
    }
}