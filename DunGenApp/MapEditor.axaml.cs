using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DunGenLib;
using System;
using System.Drawing;
using System.IO;

namespace DunGenApp;

public partial class MapEditor : Window
{
    public Map map;
    public TextureOptions textures;
    private byte dispSize = 32;
    private ushort xPos = 0;
    private ushort yPos = 0;
    private const byte IMAGE_SCALE = 4;
    private byte tileType = 0;
    public byte DispSize { get => dispSize; set => dispSize = value; }
    public MapEditor(Map m, TextureOptions t)
    {
        map = m;
        textures = t;
        EditArea = new();
        PosMarker = new Avalonia.Controls.Shapes.Rectangle { Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 0, 0)), Opacity = 0.5 };
        InitializeComponent();
        TileSelect.Height = textures.TileSize;
        MenuItem mi = new MenuItem
        {
            Width = textures.TileSize,
            Height = textures.TileSize,
            Padding = new Thickness(0),
            Margin = new Thickness(5, 0),
            Header = new Avalonia.Controls.Image { Source = textures.Tiles[textures.gen.WallID], Stretch = Stretch.Uniform },
            Tag = textures.gen.WallID,
        };
        ToolTip.SetTip(mi, "Wall");
        mi.Click += SelectTile;
        TileSelect.Items.Add(mi);
        foreach (GroundOptions g in textures.gen.GroundIDs)
        {
            mi = new MenuItem
            {
                Width = textures.TileSize,
                Height = textures.TileSize,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0),
                Header = new Avalonia.Controls.Image { Source = textures.Tiles[g.id], Stretch = Stretch.Uniform },
                Tag = g.id,
            };
            ToolTip.SetTip(mi, g.tag);
            mi.Click += SelectTile;
            TileSelect.Items.Add(mi);
            ReloadDisp();
        }
        foreach (PoolOptions p in textures.gen.PoolIDs)
        {
            mi = new MenuItem
            {
                Width = textures.TileSize,
                Height = textures.TileSize,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0),
                Header = new Avalonia.Controls.Image { Source = textures.Tiles[p.id], Stretch = Stretch.Uniform },
                Tag = p.id,
            };
            ToolTip.SetTip(mi, p.tag);
            mi.Click += SelectTile;
            TileSelect.Items.Add(mi);
        }
        SelectDisp.Width = textures.TileSize;
        SelectDisp.Height = textures.TileSize;
        ReloadDisp();
    }
    private void SelectTile(object? source, RoutedEventArgs e)
    {
        MenuItem b = (MenuItem)(source ?? new MenuItem());
        try
        {
            tileType = (byte)(b.Tag ?? 0);
            SelectDisp.Header = new Avalonia.Controls.Image { Source = textures.Tiles[tileType], Stretch = Stretch.Uniform, Width = textures.TileSize, Height = textures.TileSize };
            ToolTip.SetTip(SelectDisp, $"Current selection: {ToolTip.GetTip(b)}");
        }
        catch
        {
            
        }
    }
    private void FillMap(object? source, RoutedEventArgs e)
    {
        textures.gen.FillMap();
    }
    private void ResizeDisp(object? source, RoutedEventArgs e)
    {
        EditArea.Children.Clear();
        EditArea.Rows = DispSize;
        EditArea.Columns = DispSize;
        EditArea.Width = textures.TileSize * DispSize;
        EditArea.Height = textures.TileSize * DispSize;
        ReloadDisp();
    }
    private void MoveDisp(object? source, RoutedEventArgs e)
    {
        Button b = (Button)(source ?? new Button());
        bool reload = false;
        switch (b.Tag)
        {
            case "L":
                reload = xPos > 0;
                if (reload) xPos -= (byte)(DispSize / 2);
                break;
            case "R":
                reload = xPos < map.width - DispSize;
                if (reload) xPos += (byte)(DispSize / 2);
                break;
            case "U":
                reload = yPos > 0;
                if (reload) yPos -= (byte)(DispSize / 2);
                break;
            case "D":
                reload = yPos < map.height - DispSize;
                if (reload) yPos += (byte)(DispSize / 2);
                break;

        }
        if (reload) ReloadDisp();
    }
    private void ReloadDisp()
    {
        EditArea.Children.Clear();
        Button b;
        for (ushort y = yPos; y < yPos + DispSize; y++)
        {
            for (ushort x = xPos; x < xPos + DispSize; x++)
            {
                b = new Button
                {
                    Width = textures.TileSize,
                    Height = textures.TileSize,
                    Content = new Avalonia.Controls.Image { Source = textures.Tiles[map.GetTile(x, y)], Stretch = Stretch.Uniform },
                    Padding = new Thickness(0),
                    Tag = y * map.width + x,
                };
                b.Click += PlaceTile;
                EditArea.Children.Add(b);
            }
        }
        PosMarker.Width = DispSize * IMAGE_SCALE;
        PosMarker.Height = DispSize * IMAGE_SCALE;
        Canvas.SetTop(PosMarker, yPos * IMAGE_SCALE);
        Canvas.SetLeft(PosMarker, xPos * IMAGE_SCALE);
    }
    private void PlaceTile(object? source, RoutedEventArgs e)
    {
        Button b = (Button)(source ?? new Button());
        try
        {
            uint index = Convert.ToUInt32(b.Tag ?? map.tiles.Length + 1);
            map.tiles[index] = tileType;
            b.Content = new Avalonia.Controls.Image { Source = textures.Tiles[tileType], Stretch = Stretch.Uniform };
        }
        catch
        {

        }
    }
    private void ToggleMap(object? source, RoutedEventArgs e)
    {
        if (FullMap.IsVisible)
        {
            FullMap.IsVisible = false;
            EditArea.IsVisible = true;
            MapDisp.Height = EditArea.Height;
        }
        else
        {
            UpdateMapImage();
            MapDisp.Height = MapImage.Height;
            FullMap.IsVisible = true;
            EditArea.IsVisible = false;
        }
    }
    private void UpdateMapImage()
    {
        Avalonia.Media.Imaging.Bitmap bmp = ConvertBitmap(VisualizeMap());
        MapImage.Source = bmp;
        MapImage.Width = bmp.Size.Width * IMAGE_SCALE;
        MapImage.Height = bmp.Size.Height * IMAGE_SCALE;
    }
    private Avalonia.Media.Imaging.Bitmap ConvertBitmap(Bitmap bmp)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            return new Avalonia.Media.Imaging.Bitmap(ms);
        }
    }
    private Bitmap VisualizeMap()
    {
        Bitmap bmp = new(map.width, map.height);
        byte b;
        int i;
        System.Drawing.Color wallCol = System.Drawing.Color.FromArgb(191, 127, 0);
        for (ushort y = 0; y < map.height; y++)
        {
            for (ushort x = 0; x < map.width; x++)
            {
                b = map.GetTile(x, y);
                i = textures.gen.InGroundIDs(b);
                if (i > -1)
                {
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, (i + 1) / textures.gen.GroundIDs.Count * 255, 0));
                    continue;
                }
                i = textures.gen.InPoolIDs(b);
                if (i > -1)
                {
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, (i + 1) / textures.gen.PoolIDs.Count * 255));
                    continue;
                }
                if (b == textures.gen.WallID) bmp.SetPixel(x, y, wallCol);
            }
        }
        return bmp;
    }
}