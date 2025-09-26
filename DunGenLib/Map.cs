using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DunGenLib
{
    public class Map
    {
        /// <summary>
        /// The tiles of the map. Each <c>byte</c> represents a tile, with the <c>byte</c> signifying the type of tile.
        /// </summary>
        /// <remarks>
        /// This array is one dimensional: use <see cref="GetTile(ushort, ushort)"/> or <see cref="PlaceTile(ushort, ushort, byte)"/> for coordinate-based access.
        /// </remarks>
        public byte[] Tiles { get; set; }
        [JsonInclude] public ushort Width { get; private set; }
        [JsonInclude] public ushort Height { get; private set; }
        /// <summary>
        /// The <c>byte</c> value that represents wall tiles.
        /// </summary>
        public byte WallID { get; set; }
        /// <summary>
        /// The <c>byte</c> values that represents ground tiles.
        /// </summary>
        public List<byte> GroundIDs { get; set; }
        /// <summary>
        /// The <c>byte</c> values that represents liquid tiles.
        /// </summary>
        public List<byte> PoolIDs { get; set; }
        /// <summary>
        /// The default value to fill the tiles array with.
        /// </summary>
        [JsonInclude] private byte DefValue { get; set; }
        /// <summary>
        /// Initializes the map with a defined width and height.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="def">The default value to fill the map with.</param>
        public Map(ushort w, ushort h, byte def = 0)
        {
            Width = w;
            Height = h;
            Tiles = new byte[Width * Height];
            DefValue = def;
            FillMap(DefValue);
            GroundIDs = [];
            PoolIDs = [];
        }
        /// <summary>
        /// Fills the entire map with the value specified.
        /// </summary>
        /// <param name="value"></param>
        public void FillMap(byte value) => Array.Fill<byte>(Tiles, value);
        /// <summary>
        /// Resizes the map to the specified width and height.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <remarks>Clears the map by filling it with its <see cref="DefValue"/></remarks>
        public void Resize(ushort w, ushort h)
        {
            Width = w;
            Height = h;
            Tiles = new byte[Width * Height];
            FillMap(DefValue);
        }
        /// <summary>
        /// Coordinate-based getter for the <see cref="Tiles"/> array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The <c>byte</c> value of the tile.</returns>
        public byte GetTile(ushort x, ushort y) => Tiles[y * Width + x];
        /// <summary>
        /// Coordinate-based setter for the <see cref="Tiles"/> array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value">The <c>byte</c> value to replace the tile with.</param>
        public void PlaceTile(ushort x, ushort y, byte value)
        {
            Tiles[y * Width + x] = value;
        }
        /// <summary>
        /// A variant of <see cref="PlaceTile(ushort, ushort, byte)"/> that inserts a tile into the specified row at the specified y-position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value">The <c>byte</c> value of the tile to be inserted.</param>
        /// <remarks>Pushes the rightmost tile of the row off the map.</remarks>
        public void InsertTile(ushort x, ushort y, byte value)
        {
            for (ushort c = (ushort)(Width - 1); c > x; c--)
            {
                PlaceTile(c, y, GetTile((ushort)(c - 1), y));
            }
            PlaceTile(x, y, value);
        }
        /// <summary>
        /// Fills a rectangular area with the specified value.
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="value"></param>
        public void CarveRect(ushort posX, ushort posY, ushort sizeX, ushort sizeY, byte value)
        {
            for (ushort y = posY; y < posY + sizeY; y++)
            {
                if (y >= Height) break;
                for (ushort x = posX; x < posX + sizeX; x++)
                {
                    if (x >= Width) break;
                    PlaceTile(x, y, value);
                }
            }
        }
        /// <summary>
        /// Gets the 3 x 3 area around a tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="corners">Include tiles diagonal to the selected tile.</param>
        /// <returns>A 3 x 3 array that contains the values of the surrounding tiles (with the selected tile at [1, 1])</returns>
        public byte?[,] GetArea(ushort x, ushort y, bool corners = true)
        {
            byte?[,] result = new byte?[3, 3];
            result[1, 1] = GetTile(x, y);
            result[0, 0] = corners && x > 0 && y > 0 ? GetTile((ushort)(x - 1), (ushort)(y - 1)) : null;
            result[0, 1] = y > 0 ? GetTile(x, (ushort)(y - 1)) : null;
            result[0, 2] = corners && x < Width - 1 && y > 0 ? GetTile((ushort)(x + 1), (ushort)(y - 1)) : null;
            result[1, 0] = x > 0 ? GetTile((ushort)(x - 1), y) : null;
            result[1, 2] = x < Width - 1 ? GetTile((ushort)(x + 1), y) : null;
            result[2, 0] = corners && x > 0 && y < Height - 1 ? GetTile((ushort)(x - 1), (ushort)(y + 1)) : null;
            result[2, 1] = y < Height - 1 ? GetTile(x, (ushort)(y + 1)) : null;
            result[2, 2] = corners && x < Width - 1 && y < Height - 1 ? GetTile((ushort)(x + 1), (ushort)(y + 1)) : null;
            return result;
        }
        /// <summary>
        /// Serializes the map into a byte array.
        /// </summary>
        /// <returns>Refer to the README for the data format.</returns>
        public byte[] Serialize() 
        {
            List<byte> data = new();
            data.Add((byte)(Width & 0b11111111));
            data.Add((byte)(Width >>> 8));
            data.Add((byte)(Height & 0b11111111));
            data.Add((byte)(Height >>> 8));
            data.AddRange(Tiles);
            return data.ToArray();
        }
        /// <summary>
        /// Serializes the map into a byte array and writes it into the specified filestream.
        /// </summary>
        /// <remarks>Refer to the README for the data format.</remarks>
        public void Serialize(FileStream fs)
        {
            fs.Position = 0;
            fs.Write(Serialize());
            fs.Close();
        }
        /// <summary>
        /// Serializes the map into a byte array and writes it into the specified path..
        /// </summary>
        /// <param name="path">THe full path of the file to write to.</param>
        /// <remarks>
        /// If the file doesn't exist, a new one is created.<br/>
        /// Refer to the README for the data format.
        /// </remarks>
        public void Serialize(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Serialize(fs);
            }
        }
        /// <summary>
        /// Deserializes a byte array into a map.
        /// </summary>
        /// <param name="data">The byte array to be processed.</param>
        /// <remarks>Refer to the README for the data format.</remarks>
        public static Map Deserialize(byte[] data)
        {
            ushort w = (ushort)(data[0] + data[1] * 256);
            ushort h = (ushort)(data[2] + data[3] * 256);
            Map m = new Map(w, h);
            for (ushort y = 0; y < h; y++)
            {
                for (ushort x = 0; x < w; x++)
                {
                    if (y * w + x + 4 >= data.Length) goto desExit;
                    else m.PlaceTile(x, y, data[y * w + x + 4]);
                }
            }
        desExit: return m;
        }
        /// <summary>
        /// Reads data from a filestream and decodes it into a map.
        /// </summary>
        /// <param name="fs">The filestream to be read from.</param>
        /// <remarks>Refer to the README for the data format.</remarks>
        public static Map Deserialize(FileStream fs)
        {
            fs.Position = 0;
            byte[] data = new byte[fs.Length];
            fs.ReadExactly(data);
            fs.Close();
            return Deserialize(data);
        }
        /// <summary>
        /// Reads data from the file path specified and decodes the data into a map.
        /// </summary>
        /// <param name="path">The full path of the file to be read from.</param>
        /// <remarks>Refer to the README for the data format.</remarks>
        public static Map Deserialize(string path)
        {
            Map m;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                m = Deserialize(fs);
            }
            return m;
        }
    }
}
