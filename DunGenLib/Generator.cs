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
        public Value2D<ushort> MapChunks = new() { x = 5, y = 5 };

        public Value2D<ushort> MinRoomSize = new() { x = 3, y = 3 };
        public Value2D<ushort> MaxRoomSize = new() { x = 16, y = 16 };
        public bool MergeRooms = false;
        public bool TouchRooms = false;
        public ushort RoomsPerChunk = 1;
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

        private List<RoomData> rooms = [];
        public void GenerateMap()
        {
            grid.FillMap(WallID);
            GenerateRooms();
            GeneratePaths();
        }
        public byte NeighbourGround(ushort x, ushort y, bool corners = true)
        {
            byte result = 0;
            foreach (byte? i in grid.GetArea(x, y, corners))
            {
                if (i == null) continue;
                if (InGroundIDs(i ?? WallID)) result++;
            }
            return result;
        }
        protected void CarveRoom(Value2D<ushort> pos, Value2D<ushort> size)
        {
            for (ushort y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= grid.height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= grid.width) break;
                    grid.PlaceTile(x, y, GroundIDs[0].id);
                }
            }
        }
        protected void GenerateRooms()
        {
            Value2D<ushort> chunkSize = new() { x = (ushort)(grid.width/MapChunks.x), y = (ushort)(grid.height/MapChunks.y) };
            Value2D<ushort> pos;
            Value2D<ushort> size;
            for (ushort v = 0; v < MapChunks.y; v++)
            {
                for (ushort h = 0; h < MapChunks.x; h++)
                {
                    for (ushort i = 0; i < RoomsPerChunk; i++)
                    {
                        do
                        {
                            pos.x = (ushort)rng.Next(chunkSize.x * h, chunkSize.x * (h + 1) - (ushort)Math.Ceiling((double)chunkSize.x / 2));
                            pos.y = (ushort)rng.Next(chunkSize.y * v, chunkSize.y * (v + 1) - (ushort)Math.Ceiling((double)chunkSize.y / 2));
                            size.x = (ushort)rng.Next(MinRoomSize.x, MaxRoomSize.x);
                            size.y = (ushort)rng.Next(MinRoomSize.y, MaxRoomSize.y);
                        } while (!MergeRooms && CheckRoom(pos, size));
                        CarveRoom(pos, size);
                        rooms.Add(new() { pos = pos, size = size });
                    }
                }
            }
        }
        protected bool CheckRoom(Value2D<ushort> pos, Value2D<ushort> size)
        {
            ushort x, y;
            Value2D<ushort> minBounds = new() { x = !TouchRooms && pos.x > 0 ? (ushort)(pos.x - 1) : pos.x, y = !TouchRooms && pos.y > 0 ? (ushort)(pos.y - 1) : pos.y };
            Value2D<ushort> maxBounds = new() { x = (ushort)(TouchRooms ? pos.x + size.x : pos.x + size.x + 1), y = (ushort)(TouchRooms ? pos.y + size.y : pos.y + size.y + 1) };
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
        protected void CarvePath(Value2D<ushort> startPos, Direction startDir)
        {

        }
        protected void GeneratePaths()
        {

        }
    }
    public struct Value2D<T>
    {
        public T x;
        public T y;
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
        public Value2D<ushort> pos;
        public Value2D<ushort> size;
    }
    public enum Direction : short
    {
        Up = 0,
        Right = 90,
        Down = 180,
        Left = 270,
    }
}
