using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGen.NET
{
    public class Grid
    {
        private byte[] tiles;
        public readonly byte width;
        public readonly byte height;
        public Grid(byte w, byte h, byte defValue = 0)
        {
            width = w;
            height = h;
            tiles = new byte[width * height];
            FillMap(defValue);
        }
        public void FillMap(byte value)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    PlaceTile(x, y, value);
                }
            }
        }
        public byte GetTile(int x, int y) => tiles[y * width + x];
        public void PlaceTile(int x, int y, byte value = 0)
        {
            tiles[y * width + x] = value;
        }
        public void InsertTile(int x, int y, byte value = 0)
        {
            for (int i = y * (width + 1) - 1; i > y * width + x; i--)
            {
                tiles[i] = tiles[i - 1];
            }
            PlaceTile(x, y, value);
        }
        public byte?[,] GetArea(int x, int y, bool corners = true)
        {
            byte?[,] result = new byte?[3, 3];
            result[1, 1] = GetTile(x, y);
            result[0, 0] = corners && x > 0 && y > 0 ? GetTile(x - 1, y - 1) : null;
            result[0, 1] = y > 0 ? GetTile(x, y - 1) : null;
            result[0, 2] = corners && x < width - 1 && y > 0 ? GetTile(x + 1, y - 1) : null;
            result[1, 0] = x > 0 ? GetTile(x - 1, y) : null;
            result[1, 2] = x < width - 1 ? GetTile(x + 1, y) : null;
            result[2, 0] = corners && x > 0 && y < height - 1 ? GetTile(x - 1, y + 1) : null;
            result[2, 1] = y < height - 1 ? GetTile(x, y + 1) : null;
            result[2, 2] = corners && x < width - 1 && y < height - 1 ? GetTile(x + 1, y + 1) : null;
            return result;
        }
    }
}
