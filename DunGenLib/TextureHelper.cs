using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunGenLib
{
    public class TextureHelper
    {
        public required Map Map { get; set; }
        public Bitmap?[] Tiles => tiles;
        private Bitmap?[] tiles = new Bitmap?[256];
        public void Serialize(string mapPath)
        {
            FileInfo f = new FileInfo(mapPath);
            Map.Serialize(mapPath);
            for (int i = 0; i < 256; i++)
            {
                if (tiles[i] != null)
                {
                    tiles[i]?.Save(ExtendPath(f.Directory, i.ToString("x2") + ".bmp"));
                }
            }
        }
        public static TextureHelper Deserialize(string mapPath)
        {
            FileInfo file = new FileInfo(mapPath);
            Map m = Map.Deserialize(mapPath);
            TextureHelper t = new TextureHelper { Map = m };
            FileInfo f;
            for (int b = 0; b < 256; b++)
            {
                f = new FileInfo(ExtendPath(file.Directory, b.ToString("x2") + ".bmp"));
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
