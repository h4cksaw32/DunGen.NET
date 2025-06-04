using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenLib
{
    public class Generator
    {
        public required Map map;
        private readonly Random rng = new();
        public Value2D<ushort> MapChunks = new() { x = 5, y = 5 };

        public Value2D<ushort> MinRoomSize = new() { x = 3, y = 3 };
        public Value2D<ushort> MaxRoomSize = new() { x = 16, y = 16 };
        public bool MergeRooms = false;
        public bool TouchRooms = false;
        public float RoomsPerChunk = 1;
        public byte MinRoomExits = 3;
        public byte MaxRoomExits = 6;

        public float PoolsPerChunk = 1;

        public float PathBend = 0.15F;
        public float PathTerminate = 0.01F;
        public bool Crossroads = true;
        public bool EndAtBoundary = true;

        public byte WallID = 0;
        public List<PoolOptions> PoolIDs = [];
        public List<GroundOptions> GroundIDs = [];
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
            foreach (GroundOptions item in GroundIDs) map.GroundIDs.Add(item.id);
            foreach (PoolOptions item in PoolIDs) map.PoolIDs.Add(item.id);
            map.FillMap(WallID);
            GenerateRooms();
            GeneratePaths();
            GeneratePools();
        }
        protected void CarveRect(Value2D<ushort> pos, Value2D<ushort> size, byte id)
        {
            for (ushort y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= map.height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= map.width) break;
                    map.PlaceTile(x, y, id);
                }
            }
        }
        protected void CarveRoom(Value2D<ushort> pos, Value2D<ushort> size)
        {
            List<GroundOptions> ids = [];
            List<float> prob = [];
            float denom = 0;
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inRooms && !item.patches) ids.Add(item);
            }
            foreach (GroundOptions item in ids)
            {
                denom += item.spawnRate;
                prob.Add(denom);
            }
            byte id = ids.Count == 0 ? GroundIDs[0].id : ids[0].id;
            float rand;
            for (ushort y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= map.height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= map.width) break;
                    if (ids.Count == 0)
                    {
                        map.PlaceTile(x, y, GroundIDs[0].id);
                    }
                    else if (ids.Count == 1)
                    {
                        map.PlaceTile(x, y, ids[0].id);
                    }
                    else
                    {
                        rand = rng.NextSingle() * denom;
                        for (int index = 0; index < prob.Count; index++)
                        {
                            if (rand < prob[index])
                            {
                                id = ids[index].id;
                                break;
                            }
                        }
                        map.PlaceTile(x, y, id);
                    }
                }
            }
            ids = [];
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inRooms && item.patches) ids.Add(item);
            }
            float f;
            Value2D<ushort> patchPos = new();
            Value2D<ushort> patchSize = new();
            foreach (GroundOptions item in ids)
            {
                f = item.patchesPerRoom;
                while (f > 0)
                {
                    if (f >= 1 || f < rng.NextSingle())
                    {
                        do
                        {
                            patchPos.x = (ushort)rng.Next(pos.x, pos.x + size.x);
                            patchPos.y = (ushort)rng.Next(pos.y, pos.y + size.y);
                            patchSize.x = (ushort)rng.Next(item.minPatchSize.x, item.maxPatchSize.x);
                            patchSize.y = (ushort)rng.Next(item.minPatchSize.y, item.maxPatchSize.y);
                        } while (patchPos.x + patchSize.x >= pos.x + size.x || patchPos.y + patchSize.y >= pos.y + size.y);
                        CarveRect(patchPos, patchSize, item.id);
                    }
                    f -= 1;
                }
            }
        }
        protected void GenerateRooms()
        {
            Value2D<ushort> chunkSize = new() { x = (ushort)(map.width/MapChunks.x), y = (ushort)(map.height/MapChunks.y) };
            Value2D<ushort> pos;
            Value2D<ushort> size;
            float f;
            for (ushort v = 0; v < MapChunks.y; v++)
            {
                for (ushort h = 0; h < MapChunks.x; h++)
                {
                    f = RoomsPerChunk;
                    while (f > 0)
                    {
                        if (f >= 1 || f < rng.NextSingle())
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
                        f -= 1;
                    }
                }
            }
        }
        protected bool CheckRoom(Value2D<ushort> pos, Value2D<ushort> size)
        {
            ushort x, y;
            Value2D<ushort> minBounds = new() { x = !TouchRooms && pos.x > 0 ? (ushort)(pos.x - 1) : pos.x, y = !TouchRooms && pos.y > 0 ? (ushort)(pos.y - 1) : pos.y };
            Value2D<ushort> maxBounds = new() { x = (ushort)(TouchRooms ? pos.x + size.x : pos.x + size.x + 1), y = (ushort)(TouchRooms ? pos.y + size.y : pos.y + size.y + 1) };
            for (y = minBounds.y; y < maxBounds.y && y < map.height; y++)
            {
                if (y >= map.height) break;
                if (TouchRooms && (y == pos.y || y == pos.y + size.y - 1))
                {
                    for (x = minBounds.x; x < maxBounds.x && x < map.width; x++)
                    {
                        if (x >= map.width) break;
                        if (InGroundIDs(map.GetTile(x, y))) return true;
                    }
                }
                else if (y < pos.y || y >= pos.y + size.y)
                {
                    for (x = pos.x; x < pos.x + size.x && x < map.width; x++)
                    {
                        if (x >= map.width) break;
                        if (InGroundIDs(map.GetTile(x, y))) return true;
                    }
                }
                else
                {
                    if (InGroundIDs(map.GetTile(minBounds.x, y))) return true;
                    else if (InGroundIDs(map.GetTile(maxBounds.x < map.width ? maxBounds.x : (ushort)(map.width - 1), y))) return true;
                }
            }
            return false;
        }
        protected void CarvePath(Value2D<ushort> pos, Direction dir)
        {
            List<GroundOptions> ids = [];
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inPaths) ids.Add(item);
            }
            List<float> prob = [];
            float denom = 0F;
            foreach (GroundOptions item in ids)
            {
                denom += item.spawnRate;
                prob.Add(denom);
            }
            if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= map.width - 1 && dir == Direction.Right) || (pos.y >= map.height - 1 && dir == Direction.Down)) return;
            pos.x = (ushort)(pos.x + DirToVec(dir).x);
            pos.y = (ushort)(pos.y + DirToVec(dir).y);
            map.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
            byte id = ids.Count == 0 ? GroundIDs[0].id : ids[0].id;
            ushort segLength = 0;
            ushort segLimit = 0;
            float rand;
            while (true)
            {
                if (rng.NextSingle() < PathBend)
                {
                    if (rng.Next(2) == 0) dir = (Direction)((short)dir - 90 % 360);
                    else dir = (Direction)((short)dir + 90 % 360);
                }
                if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= map.width - 1 && dir == Direction.Right) || (pos.y >= map.height - 1 && dir == Direction.Down))
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
                    if (InGroundIDs(map.GetTile(pos.x, pos.y))) return;
                }
                if (ids.Count == 0)
                {
                    map.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
                }
                else if (ids.Count == 1)
                {
                    map.PlaceTile(pos.x, pos.y, ids[0].id);
                }
                else
                {
                    if (segLength == 0)
                    {
                        rand = rng.NextSingle() * denom;
                        int index;
                        for (index = 0; index < prob.Count; index++)
                        {
                            if (rand < prob[index])
                            {
                                id = ids[index].id;
                                break;
                            }
                        }
                        if (ids[index].segments) 
                        {
                            segLimit = (ushort)rng.Next(ids[index].minSegmentLength, ids[index].maxSegmentLength + 1);
                            segLength = 1;
                        }
                        map.PlaceTile(pos.x, pos.y, id);
                    }
                    else
                    {
                        map.PlaceTile(pos.x, pos.y, id);
                        segLength = (ushort)(segLength + 1 == segLimit ? 0 : segLength + 1);
                    }
                }
                if (rng.NextSingle() < PathTerminate) return;
            }
        }
        protected void GeneratePools()
        {
            Value2D<ushort> pos = new();
            List<float> prob = [];
            float denom = 0F;
            foreach (PoolOptions item in PoolIDs)
            {
                denom += item.spawnRate;
                prob.Add(denom);
            }
            int index;
            float rand;
            for (uint i = 0; i < (uint)(MapChunks.x * MapChunks.y * RoomsPerChunk); i++)
            {
                do
                {
                    pos.x = (ushort)rng.Next(map.width);
                    pos.y = (ushort)rng.Next(map.height);
                } while (map.GetTile(pos.x, pos.y) != WallID);
                rand = rng.NextSingle() * denom;
                for (index = 0; index < prob.Count; index++)
                {
                    if (rand < prob[index]) break;
                }
                FillPool(pos.x, pos.y, PoolIDs[index]);
            }
        }
        protected void FillPool(ushort x, ushort y, PoolOptions options)
        {
            map.PlaceTile(x, y, options.id);
            if (x > 0 && map.GetTile((ushort)(x - 1), y) == WallID && rng.NextSingle() < options.spread) FillPool((ushort)(x - 1), y, options);
            if (y > 0 && map.GetTile(x, (ushort)(y - 1)) == WallID && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y - 1), options);
            if (x < map.width - 1 && map.GetTile((ushort)(x + 1), y) == WallID && rng.NextSingle() < options.spread) FillPool((ushort)(x + 1), y, options);
            if (y < map.height - 1 && map.GetTile(x, (ushort)(y + 1)) == WallID && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y + 1), options);
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
            byte?[,] area = map.GetArea(x, y);
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
                    while ((r.pos.x == 0 && dir == Direction.Left) || (r.pos.y == 0 && dir == Direction.Up) || (r.pos.x + r.size.x >= map.width - 1 && dir == Direction.Right) || (r.pos.y + r.size.y >= map.height - 1 && dir == Direction.Down))
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
        public float spread;
        public string tag;
    }
    public struct GroundOptions
    {
        public byte id;
        public float spawnRate;
        public bool inRooms;
        public bool patches;
        public float patchesPerRoom;
        public Value2D<ushort> minPatchSize;
        public Value2D<ushort> maxPatchSize;
        public bool inPaths;
        public bool segments;
        public ushort minSegmentLength;
        public ushort maxSegmentLength;
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
