using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DunGenLib;
using System;
using System.Drawing;
using System.IO;

namespace DunGenApp;

public partial class MapEditor : Window
{
    public Map map;
    public TextureInfo textures;
    //Tile editor location and size
    private byte dispSize = 32;
    private ushort xPos = 0;
    private ushort yPos = 0;
    private byte IMAGE_SCALE = 4; // Scale factor for the full map display
    private byte tileType = 0; //ID of the currently selected tile on the selection bar
    public byte DispSize { get => dispSize; set => dispSize = value; }
    public MapEditor(TextureInfo t)
    {
        map = t.map;
        textures = t;
        EditArea = new();
        PosMarker = new Avalonia.Controls.Shapes.Rectangle { Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 0, 0)), Opacity = 0.5 };
        InitializeComponent();
        //Populate selection bar
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
    private async void LoadMap(object source, RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select Map Data",
            AllowMultiple = false
        });
        if (files.Count > 0)
        {
            textures = TextureInfo.Deserialize(files[0].Path.AbsolutePath);
        }
        MapEditor w = new MapEditor(textures);
        w.Show();
        Close();
    }
    private async void SaveMap(object source, RoutedEventArgs e)
    {
        var file = await TopLevel.GetTopLevel(this)?.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Map Data",
            DefaultExtension = ".dat"
        });
        if (file != null) textures.Serialize(file.Path.AbsolutePath);
    }
    /// <summary>
    /// Handles selecting a tile from the selection bar
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void SelectTile(object? source, RoutedEventArgs e)
    {
        MenuItem b = (MenuItem)(source ?? new MenuItem());
        tileType = (byte)(b.Tag ?? 0);
        SelectDisp.Header = new Avalonia.Controls.Image { Source = textures.Tiles[tileType], Stretch = Stretch.Uniform, Width = textures.TileSize, Height = textures.TileSize };
        ToolTip.SetTip(SelectDisp, $"Current selection: {ToolTip.GetTip(b)}");
    }
    private void PlaceTile(object? source, RoutedEventArgs e)
    {
        Button b = (Button)(source ?? new Button());
        try
        {
            uint index = Convert.ToUInt32(b.Tag ?? map.Tiles.Length + 1);
            map.Tiles[index] = tileType;
            b.Content = new Avalonia.Controls.Image { Source = textures.Tiles[tileType], Stretch = Stretch.Uniform };
        }
        catch
        {

        }
    }
    private void FillMap(object? source, RoutedEventArgs e)
    {
        textures.gen.FillMap();
    }
    /// <summary>
    /// Resizes and reloads the tile editor display area when the size is changed
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void ResizeDisp(object? source, RoutedEventArgs e)
    {
        EditArea.Children.Clear();
        EditArea.Rows = DispSize;
        EditArea.Columns = DispSize;
        EditArea.Width = textures.TileSize * DispSize;
        EditArea.Height = textures.TileSize * DispSize;
        ReloadDisp();
    }
    /// <summary>
    /// Handles shifting the tile editor display area
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
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
                reload = xPos < map.Width - DispSize;
                if (reload) xPos += (byte)(DispSize / 2);
                break;
            case "U":
                reload = yPos > 0;
                if (reload) yPos -= (byte)(DispSize / 2);
                break;
            case "D":
                reload = yPos < map.Height - DispSize;
                if (reload) yPos += (byte)(DispSize / 2);
                break;

        }
        if (reload) ReloadDisp();
    }
    /// <summary>
    /// Clear and repopulate the tile editor display area
    /// </summary>
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
                    Tag = y * map.Width + x,
                };
                b.Click += PlaceTile;
                ToolTip.SetTip(b, $"{x}, {y}");
                EditArea.Children.Add(b);
            }
        }
        PosMarker.Width = DispSize * IMAGE_SCALE;
        PosMarker.Height = DispSize * IMAGE_SCALE;
        Canvas.SetTop(PosMarker, yPos * IMAGE_SCALE);
        Canvas.SetLeft(PosMarker, xPos * IMAGE_SCALE);
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
    /// <summary>
    /// Generates a bitmap visualization of the entire map
    /// </summary>
    /// <returns></returns>
    private Bitmap VisualizeMap()
    {
        Bitmap bmp = new(map.Width, map.Height);
        byte b;
        int i;
        System.Drawing.Color wallCol = System.Drawing.Color.FromArgb(191, 127, 0);
        for (ushort y = 0; y < map.Height; y++)
        {
            for (ushort x = 0; x < map.Width; x++)
            {
                b = map.GetTile(x, y);
                if (b == textures.gen.WallID) bmp.SetPixel(x, y, wallCol);
                else
                {
                    i = textures.gen.InGroundIDs(b);
                    if (i > -1)
                    {
                        bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, (textures.gen.GroundIDs.Count - i) * 255 / textures.gen.GroundIDs.Count, 0));
                        continue;
                    }
                    i = textures.gen.InPoolIDs(b);
                    if (i > -1)
                    {
                        bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, (textures.gen.PoolIDs.Count - i) * 255 / textures.gen.PoolIDs.Count));
                        continue;
                    }
                }
            }
        }
        return bmp;
    }
    /// <summary>
    /// Updates the full map image display
    /// </summary>
    private void UpdateMapImage()
    {
        Avalonia.Media.Imaging.Bitmap bmp = ConvertBitmap(VisualizeMap());
        MapImage.Source = bmp;
        MapImage.Width = bmp.Size.Width * IMAGE_SCALE;
        MapImage.Height = bmp.Size.Height * IMAGE_SCALE;
        FullMap.Width = MapImage.Width;
        FullMap.Height = MapImage.Height;
    }
    /// <summary>
    /// Converts <see cref="System.Drawing.Bitmap"/> to <see cref="Avalonia.Media.Imaging.Bitmap"/>.
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    private Avalonia.Media.Imaging.Bitmap ConvertBitmap(Bitmap bmp)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            return new Avalonia.Media.Imaging.Bitmap(ms);
        }
    }
}