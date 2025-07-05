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

        public ushort LoopAttempts { get => loopAttempts; set => loopAttempts = value; }
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
        public List<PoolOptions> PoolIDs { get => poolIDs; set => poolIDs = value; }
        public List<GroundOptions> GroundIDs { get => groundIDs; set => groundIDs = value; }

        protected readonly Random rng = new();
        protected ushort loopAttempts = 20;
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
        protected List<PoolOptions> poolIDs = [new PoolOptions { id = 2, spawnRate = 1, spread = 0.5F }];
        protected List<GroundOptions> groundIDs = [new GroundOptions { id = 1, inPaths = true, inRooms = true, spawnRate = 1 }];
        public bool InPoolIDs(byte value)
        {
            foreach (PoolOptions i in PoolIDs)
            {
                if (i.id == value) return true;
            }
            return false;
        }
        public bool InGroundIDs(byte value)
        {
            foreach (GroundOptions i in GroundIDs)
            {
                if (i.id == value) return true;
            }
            return false;
        }
        public byte? FindVacantTileID()
        {
            byte? id = 0;
            bool[] available = new bool[256];
            Array.Fill<bool>(available, false);
            available[WallID] = true;
            foreach (GroundOptions g in GroundIDs)
            {
                available[g.id] = true;
            }
            foreach (PoolOptions p in PoolIDs)
            {
                available[p.id] = true;
            }
            checked
            {
                try
                {
                    while (available[id ?? 0]) id++;
                }
                catch (OverflowException)
                {
                    id = null;
                }
            }
            return id;
        }

        protected List<RoomData> rooms = [];

        public void GenerateMap()
        {
            Map.GroundIDs.Clear();
            Map.PoolIDs.Clear();
            ValidateIDs();
            foreach (GroundOptions item in GroundIDs) Map.GroundIDs.Add(item.id);
            foreach (PoolOptions item in PoolIDs) Map.PoolIDs.Add(item.id);
            Map.FillMap(WallID);
            GenerateRooms();
            GeneratePaths();
            GeneratePools();
        }
        public virtual void ValidateIDs()
        {
            if (InGroundIDs(WallID)) WallID = (byte)(FindVacantTileID() ?? (PoolIDs.Count > 0 ? PoolIDs[0].id : 0));
            if (InPoolIDs(WallID)) WallID = (byte)(FindVacantTileID() ?? (PoolIDs.Count > 0 ? PoolIDs[0].id : 0));
            if (GroundIDs.Count == 0) GroundIDs.Add(new GroundOptions { id = FindVacantTileID() ?? (byte)((WallID + 1) & 0b11111111), inPaths = true, inRooms = true, spawnRate = 1 });
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
            Point2D_16 patchPos;
            Point2D_16 patchSize;
            ushort counter;
            foreach (GroundOptions item in ids)
            {
                f = item.patchesPerRoom;
                while (f > 0)
                {
                    if (f >= 1 || f < rng.NextSingle())
                    {
                        counter = 0;
                        do
                        {
                            counter++;
                            patchPos = new();
                            patchSize = new();
                            patchPos.x = (ushort)rng.Next(pos.x, pos.x + size.x);
                            patchPos.y = (ushort)rng.Next(pos.y, pos.y + size.y);
                            patchSize.x = (ushort)rng.Next(item.minPatchSize.x, item.maxPatchSize.x);
                            patchSize.y = (ushort)rng.Next(item.minPatchSize.y, item.maxPatchSize.y);
                        } while (counter <= loopAttempts && (patchPos.x + patchSize.x >= pos.x + size.x || patchPos.y + patchSize.y >= pos.y + size.y));
                        if (counter < loopAttempts) CarveRect(patchPos, patchSize, item.id);
                    }
                    f -= 1;
                }
            }
        }
        protected void GenerateRooms()
        {
            rooms.Clear();
            Point2D_16 chunkSize = new() { x = (ushort)(Map.width/MapChunks.x), y = (ushort)(Map.height/MapChunks.y) };
            Point2D_16 pos;
            Point2D_16 size;
            float f;
            ushort counter;
            for (ushort v = 0; v < MapChunks.y; v++)
            {
                for (ushort h = 0; h < MapChunks.x; h++)
                {
                    f = RoomsPerChunk;
                    while (f > 0)
                    {
                        if (f >= 1 || f < rng.NextSingle())
                        {
                            counter = 0;
                            do
                            {
                                counter++;
                                size = new();
                                pos = new();
                                size.x = (ushort)rng.Next(MinRoomSize.x, MaxRoomSize.x + 1);
                                size.y = (ushort)rng.Next(MinRoomSize.y, MaxRoomSize.y + 1);
                                pos.x = size.x >= chunkSize.x ? (ushort)(Map.width / MapChunks.x * h) : (ushort)rng.Next(Map.width / MapChunks.x * h, Map.width / MapChunks.x * (h + 1) - size.x);
                                pos.y = size.y >= chunkSize.y ? (ushort)(Map.height / MapChunks.y * v) : (ushort)rng.Next(Map.height / MapChunks.y * v, Map.height / MapChunks.y * (v + 1) - size.y);
                            } while (counter <= loopAttempts && !MergeRooms && CheckRoomOverlap(pos, size));
                            if (counter <= loopAttempts)
                            {
                                CarveRoom(pos, size);
                                rooms.Add(new() { pos = pos, size = size });
                            }
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
            if (PoolIDs.Count == 0) return;
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
    public class Point2D_16
    {
        protected ushort fx;
        public ushort x
        {
            get => fx;
            set => fx = value;
        }
        protected ushort fy;
        public ushort y
        {
            get => fy;
            set => fy = value;
        }
    }
    public class Vec2D_8
    {
        protected sbyte fx;

        public sbyte x
        {
            get => fx;
            set => fx = value;
        }
        protected sbyte fy;
        public sbyte y
        {
            get => fy; 
            set => fy = value;
        }
    }
    public class TileOptions
    {
        public byte id { get; set; }
        public float spawnRate { get => spawnRateF; set => spawnRateF = value; }
        public string tag { get => tagF; set => tagF = value; }

        protected string tagF = "";
        private float spawnRateF = 1;
    }
    public class PoolOptions : TileOptions
    {
        public float spread { get; set; }
    }
    public class GroundOptions : TileOptions
    {
        public bool inRooms { get => inRoomsF; set => inRoomsF = value; }
        protected bool inRoomsF = true;
        public bool patches { get => patchesF; set => patchesF = value; }
        protected bool patchesF = false;
        public float patchesPerRoom { get; set; }
        public Point2D_16 minPatchSize { get => mnps; set => mnps = value; }
        protected Point2D_16 mnps = new() { x = 1, y = 1 };
        public Point2D_16 maxPatchSize { get => mxps; set => mxps = value; }
        protected Point2D_16 mxps = new() { x = 1, y = 1 };
        public bool inPaths { get => inPathsF; set => inPathsF = value; }
        protected bool inPathsF = true;
        public bool segments { get; set; }
        public ushort minSegmentLength { get; set; }
        public ushort maxSegmentLength { get; set; }
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
