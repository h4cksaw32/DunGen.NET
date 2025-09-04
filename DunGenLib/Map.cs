using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenLib
{
    public class Map
    {
        public byte[] tiles;
        public ushort width { get; private set; }
        public ushort height { get; private set; }
        public byte WallID;
        public List<byte> GroundIDs = [];
        public List<byte> PoolIDs = [];
        private byte defValue;
        public Map(ushort w, ushort h, byte def = 0)
        {
            width = w;
            height = h;
            tiles = new byte[width * height];
            defValue = def;
            FillMap(defValue);
        }
        public void FillMap(byte value)
        {
            for (ushort y = 0; y < height; y++)
            {
                for (ushort x = 0; x < width; x++)
                {
                    PlaceTile(x, y, value);
                }
            }
        }
        public void Resize(ushort w, ushort h)
        {
            width = w;
            height = h;
            tiles = new byte[width * height];
            FillMap(defValue);
        }
        public byte GetTile(ushort x, ushort y) => tiles[y * width + x];
        public void PlaceTile(ushort x, ushort y, byte value)
        {
            tiles[y * width + x] = value;
        }
        public void InsertTile(ushort x, ushort y, byte value)
        {
            for (ushort c = (ushort)(width - 1); c > x; c--)
            {
                PlaceTile(x, y, GetTile((ushort)(c - 1), y));
            }
            PlaceTile(x, y, value);
        }
        public byte?[,] GetArea(ushort x, ushort y, bool corners = true)
        {
            byte?[,] result = new byte?[3, 3];
            result[1, 1] = GetTile(x, y);
            result[0, 0] = corners && x > 0 && y > 0 ? GetTile((ushort)(x - 1), (ushort)(y - 1)) : null;
            result[0, 1] = y > 0 ? GetTile(x, (ushort)(y - 1)) : null;
            result[0, 2] = corners && x < width - 1 && y > 0 ? GetTile((ushort)(x + 1), (ushort)(y - 1)) : null;
            result[1, 0] = x > 0 ? GetTile((ushort)(x - 1), y) : null;
            result[1, 2] = x < width - 1 ? GetTile((ushort)(x + 1), y) : null;
            result[2, 0] = corners && x > 0 && y < height - 1 ? GetTile((ushort)(x - 1), (ushort)(y + 1)) : null;
            result[2, 1] = y < height - 1 ? GetTile(x, (ushort)(y + 1)) : null;
            result[2, 2] = corners && x < width - 1 && y < height - 1 ? GetTile((ushort)(x + 1), (ushort)(y + 1)) : null;
            return result;
        }
        public byte[] Serialize() 
        {
            List<byte> data = new();
            data.Add((byte)(width & 0b11111111));
            data.Add((byte)(width >>> 8));
            data.Add((byte)(height & 0b11111111));
            data.Add((byte)(height >>> 8));
            data.AddRange(tiles);
            return data.ToArray();
        }
        public void Serialize(FileStream fs)
        {
            fs.Position = 0;
            fs.Write(Serialize());
            fs.Close();
        }
        public void Serialize(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Serialize(fs);
            }
        }
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
        public static Map Deserialize(FileStream fs)
        {
            fs.Position = 0;
            byte[] data = new byte[fs.Length];
            fs.ReadExactly(data);
            fs.Close();
            return Deserialize(data);
        }
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
