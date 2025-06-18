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
        public required Map Map { get; set; }

        public Point2D_16 MapChunks { get => mapChunks; set => mapChunks = value; }
        public Point2D_16 MinRoomSize { get => minRoomSize; set => minRoomSize = value; }
        public Point2D_16 MaxRoomSize { get => maxRoomSize; set => maxRoomSize = value; }
        public bool MergeRooms { get => mergeRooms; set => mergeRooms = value; }
        public bool TouchRooms { get => touchRooms; set => touchRooms = value; }
        public float RoomsPerChunk { get => roomsPerChunk; set => roomsPerChunk = value; }
        public byte MinRoomExits { get => minRoomExits; set => minRoomExits = value; }
        public byte MaxRoomExits { get => maxRoomExits; set => maxRoomExits = value; }
        public float PoolsPerChunk { get => poolsPerChunk; set => poolsPerChunk = value; }
        public float PathBend { get => pathBend; set => pathBend = value; }
        public float PathTerminate { get => pathTerminate; set => pathTerminate = value; }
        public bool Crossroads { get => crossroads; set => crossroads = value; }
        public bool EndAtBoundary { get => endAtBoundary; set => endAtBoundary = value; }
        public byte WallID { get => wallID; set => wallID = value; }
        public IList<PoolOptions> PoolIDs { get => poolIDs; set => poolIDs = value; }
        public IList<GroundOptions> GroundIDs { get => groundIDs; set => groundIDs = value; }

        protected readonly Random rng = new();
        protected Point2D_16 mapChunks = new() { x = 5, y = 5 };

        protected Point2D_16 minRoomSize = new() { x = 3, y = 3 };
        protected Point2D_16 maxRoomSize = new() { x = 16, y = 16 };
        protected bool mergeRooms = false;
        protected bool touchRooms = false;
        protected float roomsPerChunk = 1;
        protected byte minRoomExits = 3;
        protected byte maxRoomExits = 6;

        protected float poolsPerChunk = 1;

        protected float pathBend = 0.15F;
        protected float pathTerminate = 0.01F;
        protected bool crossroads = true;
        protected bool endAtBoundary = true;

        protected byte wallID = 0;
        protected IList<PoolOptions> poolIDs = [];
        protected IList<GroundOptions> groundIDs = [];
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

        protected List<RoomData> rooms = [];
        public void GenerateMap()
        {
            foreach (GroundOptions item in GroundIDs) Map.GroundIDs.Add(item.id);
            foreach (PoolOptions item in PoolIDs) Map.PoolIDs.Add(item.id);
            Map.FillMap(WallID);
            GenerateRooms();
            GeneratePaths();
            GeneratePools();
        }
        protected void CarveRect(Point2D_16 pos, Point2D_16 size, byte id)
        {
            for (ushort y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= Map.height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= Map.width) break;
                    Map.PlaceTile(x, y, id);
                }
            }
        }
        protected void CarveRoom(Point2D_16 pos, Point2D_16 size)
        {
            IList<GroundOptions> ids = [];
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
                if (y >= Map.height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (x >= Map.width) break;
                    if (ids.Count == 0)
                    {
                        Map.PlaceTile(x, y, GroundIDs[0].id);
                    }
                    else if (ids.Count == 1)
                    {
                        Map.PlaceTile(x, y, ids[0].id);
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
                        Map.PlaceTile(x, y, id);
                    }
                }
            }
            ids = [];
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inRooms && item.patches) ids.Add(item);
            }
            float f;
            Point2D_16 patchPos = new();
            Point2D_16 patchSize = new();
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
            Point2D_16 chunkSize = new() { x = (ushort)(Map.width/MapChunks.x), y = (ushort)(Map.height/MapChunks.y) };
            Point2D_16 pos = new();
            Point2D_16 size = new();
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
                                size.x = (ushort)rng.Next(MinRoomSize.x, MaxRoomSize.x);
                                size.y = (ushort)rng.Next(MinRoomSize.y, MaxRoomSize.y);
                                pos.x = (ushort)rng.Next(chunkSize.x * h, chunkSize.x * (h + 1) - size.x - 1);
                                pos.y = (ushort)rng.Next(chunkSize.y * v, chunkSize.y * (v + 1) - size.y - 1);
                            } while (!MergeRooms && CheckRoomOverlap(pos, size));
                            CarveRoom(pos, size);
                            rooms.Add(new() { pos = pos, size = size });
                        }
                        f -= 1;
                    }
                }
            }
        }
        protected bool CheckRoomOverlap(Point2D_16 pos, Point2D_16 size)
        {
            foreach (RoomData r in rooms)
            {
                if (TouchRooms)
                {
                    if ((r.pos.x <= pos.x && pos.x < r.pos.x + r.size.x) && (r.pos.y <= pos.y && pos.y < r.pos.y + r.size.y) ||
                        (pos.x <= r.pos.x && r.pos.x < pos.x + size.x) && (pos.y <= r.pos.y && r.pos.y < pos.y + size.y)) return true;
                }
                else {
                    if ((r.pos.x <= pos.x && pos.x <= r.pos.x + r.size.x) && (r.pos.y <= pos.y && pos.y <= r.pos.y + r.size.y) ||
                        (pos.x <= r.pos.x && r.pos.x <= pos.x + size.x) && (pos.y <= r.pos.y && r.pos.y <= pos.y + size.y)) return true;
                }
            }
            return false;
        }
        protected void CarvePath(Point2D_16 pos, Direction dir)
        {
            IList<GroundOptions> ids = [];
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
            if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= Map.width - 1 && dir == Direction.Right) || (pos.y >= Map.height - 1 && dir == Direction.Down)) return;
            pos.x = (ushort)(pos.x + DirToVec(dir).x);
            pos.y = (ushort)(pos.y + DirToVec(dir).y);
            Map.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
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
                if ((pos.x == 0 && dir == Direction.Left) || (pos.y == 0 && dir == Direction.Up) || (pos.x >= Map.width - 1 && dir == Direction.Right) || (pos.y >= Map.height - 1 && dir == Direction.Down))
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
                    if (InGroundIDs(Map.GetTile(pos.x, pos.y))) return;
                }
                if (ids.Count == 0)
                {
                    Map.PlaceTile(pos.x, pos.y, GroundIDs[0].id);
                }
                else if (ids.Count == 1)
                {
                    Map.PlaceTile(pos.x, pos.y, ids[0].id);
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
                        Map.PlaceTile(pos.x, pos.y, id);
                    }
                    else
                    {
                        Map.PlaceTile(pos.x, pos.y, id);
                        segLength = (ushort)(segLength + 1 == segLimit ? 0 : segLength + 1);
                    }
                }
                if (rng.NextSingle() < PathTerminate) return;
            }
        }
        protected void GeneratePools()
        {
            Point2D_16 pos = new();
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
                    pos.x = (ushort)rng.Next(Map.width);
                    pos.y = (ushort)rng.Next(Map.height);
                } while (Map.GetTile(pos.x, pos.y) != WallID);
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
            Map.PlaceTile(x, y, options.id);
            if (x > 0 && Map.GetTile((ushort)(x - 1), y) == WallID && rng.NextSingle() < options.spread) FillPool((ushort)(x - 1), y, options);
            if (y > 0 && Map.GetTile(x, (ushort)(y - 1)) == WallID && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y - 1), options);
            if (x < Map.width - 1 && Map.GetTile((ushort)(x + 1), y) == WallID && rng.NextSingle() < options.spread) FillPool((ushort)(x + 1), y, options);
            if (y < Map.height - 1 && Map.GetTile(x, (ushort)(y + 1)) == WallID && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y + 1), options);
        }
        protected static Vec2D_8 DirToVec(Direction dir)
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
            byte?[,] area = Map.GetArea(x, y);
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
                    while ((r.pos.x == 0 && dir == Direction.Left) || (r.pos.y == 0 && dir == Direction.Up) || (r.pos.x + r.size.x >= Map.width - 1 && dir == Direction.Right) || (r.pos.y + r.size.y >= Map.height - 1 && dir == Direction.Down))
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
    public struct Point2D_16
    {
        public ushort x { get; set; }
        public ushort y { get; set; }
    }
    public struct Vec2D_8
    {
        public sbyte x { get; set; }
        public sbyte y { get; set; }
    }
    public class PoolOptions
    {
        public byte id; 
        public float spawnRate;
        public float spread;
        public string tag = "";
    }
    public class GroundOptions
    {
        public byte id;
        public float spawnRate;
        public bool inRooms = true;
        public bool patches;
        public float patchesPerRoom;
        public Point2D_16 minPatchSize;
        public Point2D_16 maxPatchSize;
        public bool inPaths = true;
        public bool segments;
        public ushort minSegmentLength;
        public ushort maxSegmentLength;
    }
    public struct RoomData
    {
        public Point2D_16 pos;
        public Point2D_16 size;
    }
    public enum Direction : short
    {
        Up = 0,
        Right = 90,
        Down = 180,
        Left = 270,
    }
}
