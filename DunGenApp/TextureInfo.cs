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
    /// <summary>
    /// A container class for managing and storing/retrieving bitmap textures within the application.
    /// </summary>
    public class TextureInfo
    {
        internal bool activated = false; // Used within the application to determine if the textures are being used.
        /// <summary>
        /// Tiles to used for the map.
        /// </summary>
        /// <remarks>The index of each image corresponds to the tile ID.</remarks>
        public Bitmap?[] Tiles => tiles;
        /// <summary>
        /// The size in pixels to scale each tile to.
        /// </summary>
        public ushort TileSize => tileSize;
        public required Generator gen { get; set; }
        public Map map => gen.Map;
        //Property fields
        private readonly Bitmap?[] tiles = new Bitmap?[256];
        private ushort tileSize = 32;
        /// <summary>
        /// Saves the map and bitmap textures into a directory.
        /// </summary>
        /// <param name="mapPath">The file to save the map into.</param>
        /// <remarks>Refer to the README for the folder structure of the data.</remarks>
        public void Serialize(string mapPath)
        {
            FileInfo f = new FileInfo(mapPath);
            map.Serialize(mapPath);
            for (int i = 0; i < 256; i++)
            {
                if (tiles[i] != null)
                {
                    tiles[i]?.Save(Path.Combine(f.DirectoryName ?? "", i.ToString("x2") + ".bmp"));
                }
            }
        }
        /// <summary>
        /// Saves the map and bitmap textures from a directory.
        /// </summary>
        /// <param name="mapPath">The file to read the map from.</param>
        /// <remarks>Refer to the README for the folder structure of the data.</remarks>
        public static TextureInfo Deserialize(string mapPath)
        {
            FileInfo file = new FileInfo(mapPath);
            Map m = Map.Deserialize(mapPath);
            TextureInfo t = new TextureInfo { gen = new Generator { Map = m } };
            FileInfo f;
            for (int b = 0; b < 256; b++)
            {
                f = new FileInfo(Path.Combine(file.DirectoryName ?? "", b.ToString("x2") + ".bmp"));
                if (f.Exists)
                {
                    t.tiles[b] = new Bitmap(f.FullName);
                }
            }
            return t;
        }
    }
}
