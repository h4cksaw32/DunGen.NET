using Avalonia.Media.Imaging;
using DunGenLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunGenApp
{
    public class TextureInfo
    {
        private readonly Bitmap?[] tiles = new Bitmap?[256];
        private readonly Bitmap[] player = new Bitmap[9];
        private ushort tileSize = 32;
        public Bitmap?[] Tiles => tiles;
        public Bitmap?[] Player => player;
        public ushort TileSize => tileSize;
        public required Generator gen { get; set; }
        public Map map => gen.Map;
        public void Serialize(DirectoryInfo dir)
        {
            map.Serialize(ExtendPath(dir, "map.dat"));
            for (byte i = 0; i < 256; i++)
            {
                if (tiles[i] != null)
                {
                    tiles[i]?.Save(ExtendPath(dir, i.ToString("x2") + ".bmp"));
                }
            }
        }
        public static TextureInfo Deserialize(DirectoryInfo dir)
        {
            Map m = Map.Deserialize(ExtendPath(dir, "map.dat"));
            TextureInfo t = new TextureInfo { gen = new Generator { Map = m } };
            FileInfo f;
            for (byte b = 0; b < 256; b++)
            {
                f = new FileInfo(ExtendPath(dir, b.ToString("x2") + ".bmp"));
                if (f.Exists)
                {
                    t.tiles[b] = new Bitmap(f.FullName);
                }
            }
            return t;
        }
        private static string ExtendPath(DirectoryInfo dir, string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"{dir.FullName}\\{fileName}";
            else return $"{dir.FullName}/{fileName}";
        }
    }
}
