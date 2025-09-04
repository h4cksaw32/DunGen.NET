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
    private TextureInfo textures;
    public TextureEditor(TextureInfo t)
    {
        textures = t;
        InitializeComponent();
        Disp.Children.Add(new TextBlock
        {
            Text = "Player",
            FontSize = 18,
            FontWeight = FontWeight.DemiBold
        });
        UniformGrid g = new UniformGrid {
            Columns = 3,
            Rows = 3,
            MinHeight = 48,
            MinWidth = 48,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        ImgListSelectButton b;
        for (int i = 0; i < 9; i++)
        {
            b = new ImgListSelectButton(i, textures.Player);
            b.MinHeight = 16;
            b.MinWidth = 16;
            g.Children.Add(b);
        }
        Disp.Children.Add(g);
        Disp.Children.Add(new TextBlock
        {
            Text = "Wall Tile",
            FontSize = 18,
            FontWeight = FontWeight.DemiBold
        });
        b = new ImgListSelectButton(textures.gen.WallID, textures.Tiles);
        b.MinHeight = 16;
        b.MinWidth = 16;
        Disp.Children.Add(b);
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
            g.Children.Add(new ImgListSelectButton(o.id, textures.Tiles));
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
            g.Children.Add(new ImgListSelectButton(o.id, textures.Tiles));
        }
        Disp.Children.Add(g);
        Disp.DataContext = textures;
    }
}
public class IndexedButton : Button
{
    public int Index { get; set; }
}