using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Layout;
using Avalonia.Media;
using DunGenLib;
using Avalonia.Media.Imaging;

namespace DunGenApp;

public partial class TextureEditor : Window
{
    //Default textures
    private static readonly Bitmap NULL_TILE = new("default.png");
    private static readonly Bitmap WALL_TILE = new("wall.png");
    private static readonly Bitmap GROUND_TILE = new("ground.png");
    private static readonly Bitmap WATER_TILE = new("water.png");
    private TextureInfo textures;
    public TextureEditor(TextureInfo t, bool flash = false) //Flash is used internally to load default textures if the map editor is opened with no textures.
    {
        textures = t;
        textures.activated = true;
        InitializeComponent();
        UniformGrid g;
        ImgListSelectButton b;
        //Add wall tile UI
        Disp.Children.Add(new TextBlock
        {
            Text = "Wall Tile",
            FontSize = 18,
            FontWeight = FontWeight.DemiBold
        });
        b = new ImgListSelectButton(textures.gen.WallID, textures.Tiles, WALL_TILE);
        b.MinHeight = 16;
        b.MinWidth = 16;
        Disp.Children.Add(b);
        //Add ground and pool tile UIs
        Disp.Children.Add(new TextBlock
        {
            Text = "Ground Tiles",
            FontSize = 18,
            FontWeight = FontWeight.DemiBold
        });
        g = new UniformGrid
        {
            Columns = 3,
            Rows = textures.gen.GroundIDs.Count + 1
        };
        g.Children.Add(new TextBlock
        {
            Text = "Tile ID",
            FontWeight = FontWeight.Medium,
        });
        g.Children.Add(new TextBlock
        {
            Text = "Name",
            FontWeight = FontWeight.Medium
        });
        g.Children.Add(new TextBlock
        {
            Text = "Image",
            FontWeight = FontWeight.Medium
        });
        foreach (GroundOptions o in textures.gen.GroundIDs)
        {
            g.Children.Add(new TextBlock
            {
                Text = o.id.ToString()
            });
            g.Children.Add(new TextBlock
            {
                Text = o.tag
            });
            g.Children.Add(new ImgListSelectButton(o.id, textures.Tiles, GROUND_TILE));
        }
        Disp.Children.Add(g);
        Disp.Children.Add(new TextBlock
        {
            Text = "Pool Tiles",
            FontSize = 18,
            FontWeight = FontWeight.DemiBold
        });
        g = new UniformGrid
        {
            Columns = 3,
            Rows = textures.gen.PoolIDs.Count + 1
        };
        g.Children.Add(new TextBlock
        {
            Text = "Tile ID",
            FontWeight = FontWeight.Medium,
        });
        g.Children.Add(new TextBlock
        {
            Text = "Name",
            FontWeight = FontWeight.Medium
        });
        g.Children.Add(new TextBlock
        {
            Text = "Image",
            FontWeight = FontWeight.Medium
        });
        foreach (PoolOptions o in textures.gen.PoolIDs)
        {
            g.Children.Add(new TextBlock
            {
                Text = o.id.ToString()
            });
            g.Children.Add(new TextBlock
            {
                Text = o.tag
            });
            g.Children.Add(new ImgListSelectButton(o.id, textures.Tiles, WATER_TILE));
        }
        Disp.Children.Add(g);
        Disp.DataContext = textures;
        if (flash)
        {
            MapEditor w = new MapEditor(textures);
            w.Show();
            Close();
        }
    }
}
public class IndexedButton : Button
{
    public int Index { get; set; }
}