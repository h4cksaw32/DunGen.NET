using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGen.NET
{
    public class Generator
    {
        public required Grid grid;
        private readonly Random rng = new();
        public Value2D<byte> MapChunks = new() { x = 5, y = 5 };

        public Value2D<byte> MinRoomSize = new() { x = 3, y = 3 };
        public Value2D<byte> MaxRoomSize = new() { x = 16, y = 16 };
        public bool MergeRooms = false;
        public bool TouchRooms = false;
        public byte RoomsPerChunk = 1;
        public byte MinRoomExits = 1;
        public byte MaxRoomExits = 4;

        public float PoolJaggedness = 0.5F;
        public bool MergePools = false;
        public byte PoolsPerChunk = 1;

        public float PathBend = 0.15F;
        public float PathTerminate = 0.05F;
        public bool Crossroads = true;
        public bool EndAtBoundary = true;

        public byte WallID = 0;
        public required List<PoolOptions> PoolIDs;
        public required List<GroundOptions> GroundIDs;
        protected bool InPoolIDs(byte value)
        {
            foreach (PoolOptions i in PoolIDs)
            {
                if (i.id == value) return true;
            }
            return false;
        }
        protected bool InGroundIDs(byte value)
        {
            foreach (GroundOptions i in GroundIDs)
            {
                if (i.id == value) return true;
            }
            return false;
        }

        private List<RoomData> rooms = new();
        public void GenerateMap()
        {
            grid.FillMap(WallID);
            GenerateRooms();
        }
        public byte NeighbourGround(byte x, byte y, bool corners = true)
        {
            byte result = 0;
            foreach (byte? i in grid.GetArea(x, y, corners))
            {
                if (i == null) continue;
                if (InGroundIDs(i ?? WallID)) result++;
            }
            return result;
        }
        protected void CarveRoom(Value2D<byte> pos, Value2D<byte> size)
        {
            for (byte y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= grid.height) break;
                for (byte x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= grid.width) break;
                    grid.PlaceTile(x, y, GroundIDs[0].id);
                }
            }
        }
        protected void GenerateRooms()
        {
            Value2D<byte> chunkSize = new() { x = (byte)(grid.width/MapChunks.x), y = (byte)(grid.height/MapChunks.y) };
            Value2D<byte> pos;
            Value2D<byte> size;
            for (byte v = 0; v < MapChunks.y; v++)
            {
                for (byte h = 0; h < MapChunks.x; h++)
                {
                    for (byte i = 0; i < RoomsPerChunk; i++)
                    {
                        do
                        {
                            pos.x = (byte)rng.Next(chunkSize.x * h, chunkSize.x * (h + 1) - (byte)Math.Ceiling((double)chunkSize.x / 2));
                            pos.y = (byte)rng.Next(chunkSize.y * v, chunkSize.y * (v + 1) - (byte)Math.Ceiling((double)chunkSize.y / 2));
                            size.x = (byte)rng.Next(MinRoomSize.x, MaxRoomSize.x);
                            size.y = (byte)rng.Next(MinRoomSize.y, MaxRoomSize.y);
                        } while (!MergeRooms && CheckRoom(pos, size));
                        CarveRoom(pos, size);
                        rooms.Add(new() { pos = pos, size = size });
                    }
                }
            }
        }
        protected bool CheckRoom(Value2D<byte> pos, Value2D<byte> size)
        {
            byte x, y;
            Value2D<byte> minBounds = new() { x = !TouchRooms && pos.x > 0 ? (byte)(pos.x - 1) : pos.x, y = !TouchRooms && pos.y > 0 ? (byte)(pos.y - 1) : pos.y };
            Value2D<byte> maxBounds = new() { x = (byte)(TouchRooms ? pos.x + size.x : pos.x + size.x + 1), y = (byte)(TouchRooms ? pos.y + size.y : pos.y + size.y + 1) };
            for (y = minBounds.y; y < maxBounds.y && y < grid.height; y++)
            {
                if (TouchRooms && (y == pos.y || y == pos.y + size.y - 1))
                {
                    for (x = minBounds.x; x < maxBounds.x && x < grid.width; x++)
                    {
                        if (InGroundIDs(grid.GetTile(x, y))) return true;
                    }
                }
                else if (y < pos.y || y >= pos.y + size.y)
                {
                    for (x = pos.x; x < pos.x + size.x && x < grid.width; x++)
                    {
                        if (InGroundIDs(grid.GetTile(x, y))) return true;
                    }
                }
                else
                {
                    if (InGroundIDs(grid.GetTile(minBounds.x, y))) return true;
                    else if (InGroundIDs(grid.GetTile(maxBounds.x, y))) return true;
                }
            }
            return false;
        }
    }
    public struct PoolOptions
    {
        public byte id; 
        public float spawnRate;
    }
    public struct GroundOptions
    {
        public byte id;
        public float spawnRate;
        public bool patched;
        public Value2D<byte> MaxPatchSize;
    }
    public struct RoomData
    {
        public Value2D<byte> pos;
        public Value2D<byte> size;
    }
    public struct Value2D<T>
    {
        public T x;
        public T y;
    }
}
