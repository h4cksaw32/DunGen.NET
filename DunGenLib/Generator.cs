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
        public byte MinRoomExits = 3;
        public byte MaxRoomExits = 6;

        public float PoolJaggedness = 0.5F;
        public bool MergePools = false;
        public byte PoolsPerChunk = 1;

        public float PathBend = 0.15F;
        public float PathTerminate = 0.01F;
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
                if (y >= grid.height) break;
                if (TouchRooms && (y == pos.y || y == pos.y + size.y - 1))
                {
                    for (x = minBounds.x; x < maxBounds.x && x < grid.width; x++)
                    {
                        if (x >= grid.width) break;
                        if (InGroundIDs(grid.GetTile(x, y))) return true;
                    }
                }
                else if (y < pos.y || y >= pos.y + size.y)
                {
                    for (x = pos.x; x < pos.x + size.x && x < grid.width; x++)
                    {
                        if (x >= grid.width) break;
                        if (InGroundIDs(grid.GetTile(x, y))) return true;
                    }
                }
                else
                {
                    if (InGroundIDs(grid.GetTile(minBounds.x, y))) return true;
                    else if (InGroundIDs(grid.GetTile(maxBounds.x < grid.width ? maxBounds.x : grid.width, y))) return true;
                }
            }
            return false;
        }
        protected void CarvePath(Value2D<ushort> pos, Direction dir)
        {
            if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= grid.width - 1 && dir == Direction.Right) || (pos.y >= grid.height - 1 && dir == Direction.Down)) return;
            pos.x = (ushort)(pos.x + DirToVec(dir).x);
            pos.y = (ushort)(pos.y + DirToVec(dir).y);
            grid.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
            while (true)
            {
                if (rng.NextSingle() < PathBend)
                {
                    if (rng.Next(2) == 0) dir = (Direction)((short)dir - 90 % 360);
                    else dir = (Direction)((short)dir + 90 % 360);
                }
                if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= grid.width - 1 && dir == Direction.Right) || (pos.y >= grid.height - 1 && dir == Direction.Down))
                {
                    if (EndAtBoundary) return;
                    else continue;
                }
                pos.x = (ushort)(pos.x + DirToVec(dir).x);
                pos.y = (ushort)(pos.y + DirToVec(dir).y);
                if (Crossroads)
                {
                    if (TileInRoom(pos.x, pos.y)) return;
                }
                else
                {
                    if (InGroundIDs(grid.GetTile(pos.x, pos.y))) return;
                }
                grid.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
                if (rng.NextSingle() < PathTerminate) return;
            }
        }
        protected static Value2D<sbyte> DirToVec(Direction dir)
        {
            return dir switch
            {
                Direction.Up => new() { x = 0, y = -1 },
                Direction.Down => new() { x = 0, y = 1 },
                Direction.Left => new() { x = -1, y = 0 },
                Direction.Right => new() { x = 1, y = 0 },
                _ => new() { x = 0, y = 0 },
            };
        }
        protected bool TileInRoom(ushort x, ushort y)
        {
            byte?[,] area = grid.GetArea(x, y);
            return (InGroundIDs(area[0, 0] ?? WallID) && InGroundIDs(area[0, 1] ?? WallID) && InGroundIDs(area[1, 0] ?? WallID))
                || (InGroundIDs(area[0, 1] ?? WallID) && InGroundIDs(area[0, 2] ?? WallID) && InGroundIDs(area[1, 2] ?? WallID))
                || (InGroundIDs(area[1, 2] ?? WallID) && InGroundIDs(area[2, 1] ?? WallID) && InGroundIDs(area[2, 2] ?? WallID))
                || (InGroundIDs(area[1, 0] ?? WallID) && InGroundIDs(area[2, 0] ?? WallID) && InGroundIDs(area[2, 1] ?? WallID));
        }
        protected void GeneratePaths()
        {
            byte exits;
            foreach (RoomData r in rooms)
            {
                exits = (byte)rng.Next(MinRoomExits, MaxRoomExits + 1);
                Direction dir = 0;
                for (int i = 0; i < exits; i++)
                {
                    while ((r.pos.x == 0 && dir == Direction.Left) || (r.pos.y == 0 && dir == Direction.Up) || (r.pos.x + r.size.x >= grid.width - 1 && dir == Direction.Right) || (r.pos.y + r.size.y >= grid.height - 1 && dir == Direction.Down))
                    {
                        dir = (Direction)(rng.Next(4) * 90);
                    }
                    switch (dir)
                    {
                        case Direction.Up:
                            CarvePath(new() { x = (ushort)(r.pos.x + rng.Next(r.size.x)), y = r.pos.y}, dir);
                            break;
                        case Direction.Down:
                            CarvePath(new() { x = (ushort)(r.pos.x + rng.Next(r.size.x)), y = (ushort)(r.pos.y + r.size.y) }, dir);
                            break;
                        case Direction.Left:
                            CarvePath(new() { x = r.pos.x, y = (ushort)(r.pos.y + rng.Next(r.size.y)) }, dir);
                            break;
                        case Direction.Right:
                            CarvePath(new() { x = (ushort)(r.pos.x + r.size.y), y = (ushort)(r.pos.y + rng.Next(r.size.y)) }, dir);
                            break;
                    }
                }
            }
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
