using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunGenLib
{
    /// <summary>
    /// A helper and container class for managing and storing/retrieving bitmap textures.
    /// </summary>
    public class TextureHelper
    {
        public required Map Map { get; set; }
        public Bitmap?[] Tiles => tiles;
        private Bitmap?[] tiles = new Bitmap?[256];
        /// <summary>
        /// Saves the map and bitmap textures into a directory.
        /// </summary>
        /// <param name="mapPath">The file to save the map into.</param>
        /// <remarks>Refer to the README for the folder structure of the data.</remarks>
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
        /// <summary>
        /// Saves the map and bitmap textures from a directory.
        /// </summary>
        /// <param name="mapPath">The file to read the map from.</param>
        /// <remarks>Refer to the README for the folder structure of the data.</remarks>
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
        /// <summary>
        /// Helper class for fusing a directory path with a file name based on the user's OS.
        /// </summary>
        /// <returns>The resulting absolute path.</returns>
        private static string ExtendPath(DirectoryInfo dir, string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"{dir.FullName}\\{fileName}";
            else return $"{dir.FullName}/{fileName}";
        }
    }
}
