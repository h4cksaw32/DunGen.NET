using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DunGenLib
{
    /// <summary>
    /// A container & processing class to generate rooms, tiles, and pools onto a <see cref="DunGenLib.Map"/> instance.
    /// </summary>
    /// <remarks>This generator specifically uses a room-placement algorithm.</remarks>
    public class Generator
    {
        /// <summary>
        /// The map to generate the features on.
        /// </summary>
        [JsonIgnore] public Map Map { get; set; }

        /// <summary>
        /// The number of attempts for generating each feature (rooms, pathways, pools).
        /// </summary>
        public ushort LoopAttempts { get => loopAttempts; set => loopAttempts = value; }
        /// <summary>
        /// Number of evenly-sized chunks to break up the map in.
        /// </summary>
        public Point2D_16 MapChunks { get => mapChunks; set => mapChunks = value; }
        /// <summary>
        /// The minimum size that a room can generated.
        /// </summary>
        public Point2D_16 MinRoomSize { get => minRoomSize; set => minRoomSize = value; }
        /// <summary>
        /// The maximum size that a room can be generated.
        /// </summary>
        public Point2D_16 MaxRoomSize { get => maxRoomSize; set => maxRoomSize = value; }
        /// <summary>
        /// Allow rooms to overlap each other.
        /// </summary>
        public bool MergeRooms { get => mergeRooms; set => mergeRooms = value; }
        /// <summary>
        /// Allow rooms to share an edge (forming a larger irregularly-shaped room).
        /// </summary>
        public bool TouchRooms { get => touchRooms; set => touchRooms = value; }
        /// <summary>
        /// Minimum number of rooms to generate in each chunk.
        /// </summary>
        public byte MinRoomsPerChunk { get => minRoomsPerChunk; set => minRoomsPerChunk = value; }
        /// <summary>
        /// Maximum number of rooms to generate in each chunk.
        /// </summary>
        public byte MaxRoomsPerChunk { get => maxRoomsPerChunk; set => maxRoomsPerChunk = value; }
        /// <summary>
        /// Minimum number of pathways to branch off of each room.
        /// </summary>
        /// <remarks>A path that connects two rooms is only attributed to one room.</remarks>
        public byte MinRoomExits { get => minRoomExits; set => minRoomExits = value; }
        /// <summary>
        /// Minimum number of pathways to branch off of each room.
        /// </summary>
        /// <remarks>A path that connects two rooms is only attributed to one room.</remarks>
        public byte MaxRoomExits { get => maxRoomExits; set => maxRoomExits = value; }
        /// <summary>
        /// Number of pools to generate in each chunk.
        /// </summary>
        /// <remarks>
        /// Unlike rooms, pools are not confined to their chunk.
        /// For each chunk, floor(x) pools are generated randomly within the map, with each additional pool having a probability of (x - floor(x)) for appearing.
        /// </remarks>
        public float PoolsPerChunk { get => poolsPerChunk; set => poolsPerChunk = value; }
        /// <summary>
        /// The probability a path turns 90 degrees for each tile it advances.
        /// </summary>
        public float PathBend { get => pathBend; set => pathBend = value; }
        /// <summary>
        /// The probability for a path to spontanenously stop generating for each tile it advances.
        /// </summary>
        /// <remarks>A value of <0.05 is recommended to avoid rooms being cut off from each other.</remarks>
        public float PathTerminate { get => pathTerminate; set => pathTerminate = value; }
        /// <summary>
        /// Allows paths to continue generating at an intersection.
        /// </summary>
        public bool Crossroads { get => crossroads; set => crossroads = value; }
        /// <summary>
        /// Stops a path from generating when it touches map boundaries.
        /// </summary>
        /// <remarks>
        /// Disabling this option simply lets the path choose another direction to generate in.</remarks>
        public bool EndAtBoundary { get => endAtBoundary; set => endAtBoundary = value; }
        /// <summary>
        /// The <c>byte</c> value in the map to represent wall tiles.
        /// </summary>
        public byte WallID { get => wallID; set => wallID = value; }
        /// <summary>
        /// Individual settings for the frequency and generation patterns for each type of ground tile.
        /// </summary>
        public List<PoolOptions> PoolIDs { get => poolIDs; set => poolIDs = value; }
        /// <summary>
        /// Individual settings for the frequency and generation patterns for each type of liquid tile.
        /// </summary>
        public List<GroundOptions> GroundIDs { get => groundIDs; set => groundIDs = value; }

        //Fields for properties above. Find the corresponding property for information.

        protected readonly Random rng = new();
        protected ushort loopAttempts = 20;
        protected Point2D_16 mapChunks = new() { x = 5, y = 5 };

        protected Point2D_16 minRoomSize = new() { x = 3, y = 3 };
        protected Point2D_16 maxRoomSize = new() { x = 16, y = 16 };
        protected bool mergeRooms = false;
        protected bool touchRooms = false;
        protected byte minRoomsPerChunk = 1;
        protected byte maxRoomsPerChunk = 1;
        protected byte minRoomExits = 3;
        protected byte maxRoomExits = 6;

        protected float poolsPerChunk = 1;

        protected float pathBend = 0.15F;
        protected float pathTerminate = 0.001F;
        protected bool crossroads = true;
        protected bool endAtBoundary = false;

        protected byte wallID = 0;
        protected List<PoolOptions> poolIDs = [new PoolOptions { id = 2, spawnRate = 1, spread = 0.5F, tag = "Water", maxPoolSize = 40000, priority = 0 }];
        protected List<GroundOptions> groundIDs = [new GroundOptions { id = 1, inPaths = true, inRooms = true, spawnRate = 1, tag = "Ground" }];
        
        //End of field definitions
        
        /// <summary>
        /// Creates a generator with a 128 x 128 map.
        /// </summary>
        public Generator()
        {
            Map = new(128, 128);
        }
        /// <summary>
        /// Creates a generator with a pre-defined map.
        /// </summary>
        /// <param name="m">The map fr the generator to use.</param>
        public Generator(Map m)
        {
            Map = m;
        }
        /// <summary>
        /// Checks if a <c>byte</c> value is used to represent a ground tile.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the index that the ID was found in <see cref="GroundIDs"/> (-1 if none)</returns>
        public int InGroundIDs(byte value)
        {
            for (int i = 0; i < GroundIDs.Count; i++)
            {
                if (GroundIDs[i].id == value) return i;
            }
            return -1;
        }
        /// <summary>
        /// Checks if a <c>byte</c> value is used to represent a liquid tile.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the index that the ID was found in <see cref="PoolIDs"/> (-1 if none)</returns>
        public int InPoolIDs(byte value)
        {
            for (int i = 0; i < PoolIDs.Count; i++)
            {
                if (PoolIDs[i].id == value) return i;
            }
            return -1;
        }
        /// <summary>
        /// Finds the lowest <c>byte</c> value that is unused in the map.
        /// </summary>
        /// <returns>The lowest unused value, or null if none are available.</returns>
        public byte? NextVacantTileID()
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
        /// <summary>
        /// Fills the map with wall tiles (value specified in <see cref="WallID"/>).
        /// </summary>
        public void FillMap() => Map.FillMap(WallID);

        /// <summary>
        /// A private buffer list for storing the locations and sizes of each room.
        /// </summary>
        protected List<RoomData> rooms = [];

        /// <summary>
        /// Generates all of the features on the map.
        /// </summary>
        public void GenerateMap()
        {
            //Flush the mirror ID indices in the map
            Map.GroundIDs.Clear();
            Map.PoolIDs.Clear();
            
            ValidateIDs();
            //Recopies the ID info to the map
            foreach (GroundOptions item in GroundIDs) Map.GroundIDs.Add(item.id);
            foreach (PoolOptions item in PoolIDs) Map.PoolIDs.Add(item.id);
            //Resets the map by filling it with wall tiles
            Map.FillMap(WallID);
            //Generate the features
            GenerateRooms();
            GeneratePaths();
            GeneratePools();
        }
        /// <summary>
        /// Makes sure that the tile settings allow successful terrain generation..
        /// </summary>
        public virtual void ValidateIDs()
        {
            // Wall ID distinct?
            if (InGroundIDs(WallID) > -1) WallID = (byte)(NextVacantTileID() ?? (PoolIDs.Count > 0 ? PoolIDs[0].id : 0));
            if (InPoolIDs(WallID) > -1) WallID = (byte)(NextVacantTileID() ?? (PoolIDs.Count > 0 ? PoolIDs[0].id : 0));
            // Ground tiles exist?
            if (GroundIDs.Count == 0) GroundIDs.Add(new GroundOptions { id = NextVacantTileID() ?? (byte)(WallID + 1), inPaths = true, inRooms = true, spawnRate = 1 });
            else
            {
                //Ground tiles allow successful generation?
                bool rooms = false;
                bool paths = false;
                foreach (GroundOptions g in GroundIDs)
                {
                    if (g.inRooms) rooms = true;
                    if (g.inPaths) paths = true;
                    if (rooms && paths) break;
                }
                if (!(rooms && paths)) GroundIDs.Add(new GroundOptions { id = NextVacantTileID() ?? (byte)(WallID + 1), inPaths = true, inRooms = true, spawnRate = 1 });
            }
            // Pool tiles are optional (sorry water lovers)
        }
        /// <summary>
        /// Wrapper method for <see cref="Map.CarveRect(ushort, ushort, ushort, ushort, byte)"/> with less parameters.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="id"></param>
        protected void CarveRect(Point2D_16 pos, Point2D_16 size, byte id) => Map.CarveRect(pos.x, pos.y, size.x, size.y, id);
        /// <summary>
        /// Generates a singular room with the specified position and size.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        protected void CarveRoom(Point2D_16 pos, Point2D_16 size)
        {
            //Draws an uniform room if only one ground tile type exists
            if (GroundIDs.Count == 1)
            {
                CarveRect(pos, size, GroundIDs[0].id);
                return;
            }
            //Buffer all ground tiles that generate in patches
            List<GroundOptions> ids = [];
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inRooms && item.patches) ids.Add(item);
            }
            float f;
            Point2D_16 patchPos;
            Point2D_16 patchSize;
            ushort counter;
            //Place patches in the room based on the "patches per room" setting for each type.
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
            //Buffer all ground tiles that generate singularly
            ids = [];
            List<float> prob = [];
            float denom = 0;
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inRooms && !item.patches) ids.Add(item);
            }
            //Make a probability table based on the spawn rate of each tile type
            foreach (GroundOptions item in ids)
            {
                denom += item.spawnRate;
                prob.Add(denom);
            }
            byte id = ids.Count == 0 ? GroundIDs[0].id : ids[0].id;
            float rand;
            for (ushort y = pos.y; y < pos.y + size.y; y++)
            {
                if (y >= Map.Height) break;
                for (ushort x = pos.x; x < pos.x + size.x; x++)
                {
                    if (InGroundIDs(Map.GetTile(x, y)) > -1) continue; //Avoid overwriting patches
                    if (x >= Map.Width) break;
                    if (ids.Count == 1)
                    {
                        Map.PlaceTile(x, y, ids[0].id); 
                    }
                    else
                    {
                        rand = rng.NextSingle() * denom;
                        //Look up the tile to place based on the probability table
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
        }
        /// <summary>
        /// Generates all the rooms in the map.
        /// </summary>
        protected void GenerateRooms()
        {
            rooms.Clear();
            Point2D_16 pos;
            Point2D_16 size;
            Point2D_16 minPos;
            Point2D_16 maxPos;
            byte r = (byte)rng.Next(minRoomsPerChunk, maxRoomsPerChunk + 1);
            ushort counter;
            for (ushort v = 0; v < MapChunks.y; v++)
            {
                for (ushort h = 0; h < MapChunks.x; h++)
                {
                    for (byte i = 0; i < r; i++)
                    {
                        counter = 0;
                        do
                        {
                            counter++;
                            size = new();
                            pos = new();
                            minPos = new();
                            maxPos = new();
                            size.x = (ushort)rng.Next(MinRoomSize.x, MaxRoomSize.x + 1);
                            size.y = (ushort)rng.Next(MinRoomSize.y, MaxRoomSize.y + 1);
                            minPos.x = (ushort)(Map.Width / MapChunks.x * h);
                            minPos.y = (ushort)(Map.Height / MapChunks.y * v);
                            maxPos.x = (ushort)(Map.Width / MapChunks.x * (h + 1) - size.x);
                            maxPos.y = (ushort)(Map.Height / MapChunks.y * (v + 1) - size.y);
                            pos.x = maxPos.x <= minPos.x ? minPos.x : (ushort)rng.Next(minPos.x, maxPos.x);
                            pos.y = maxPos.y <= minPos.y ? minPos.y : (ushort)rng.Next(minPos.y, maxPos.y);
                        } while (counter <= loopAttempts && !MergeRooms && CheckRoomOverlap(pos, size));
                        if (counter <= loopAttempts)
                        {
                            CarveRoom(pos, size);
                            rooms.Add(new() { pos = pos, size = size });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Generates a singular path from an initial position and direction.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <param name="startRoom"></param>
        protected PathGenResult CarvePath(Point2D_16 pos, Direction dir, RoomData startRoom)
        {
            //Buffer all ground tiles that occur in paths
            List<GroundOptions> ids = [];
            foreach (GroundOptions item in GroundIDs)
            {
                if (item.inPaths) ids.Add(item);
            }
            //Build the probability table based on the spawn rate of each tile type
            List<float> prob = [];
            float denom = 0F;
            foreach (GroundOptions item in ids)
            {
                denom += item.spawnRate;
                prob.Add(denom);
            }
            //Terminate paths that can't generate
            if ((pos.x == 0 && dir == Direction.W) || (pos.y == 0 && dir == Direction.N) || (pos.x >= Map.Width - 1 && dir == Direction.E) || (pos.y >= Map.Height - 1 && dir == Direction.S)) return PathGenResult.CanNotGenerate;
            //Place the first tile
            pos.x = (ushort)(pos.x + DirToVec(dir).x);
            pos.y = (ushort)(pos.y + DirToVec(dir).y);
            Map.PlaceTile(pos.x, pos.y, ids[0].id);
            //Set up local variables
            byte id = ids.Count == 0 ? GroundIDs[0].id : ids[0].id;
            ushort segLength = 0;
            ushort segLimit = 0;
            float rand;
            while (true)
            {
                if (EndAtBoundary)
                {
                    if (pos.x == 0 || pos.y == 0 || pos.x >= Map.Width - 1 || pos.y >= Map.Height - 1) return PathGenResult.EndAtBoundary;
                }
                //Bend path if neccessary, correct paths about to leave map bounds
                if (rng.NextSingle() < PathBend)
                {
                    if (rng.Next(2) == 0) dir = (Direction)((short)(dir - 90) % 360);
                    else dir = (Direction)((short)(dir + 90) % 360);
                }
                if (!EndAtBoundary)
                {
                    if ((pos.x == 0 && dir == Direction.W) || (pos.y == 0 && dir == Direction.N) || (pos.x >= Map.Width - 1 && dir == Direction.E) || (pos.y >= Map.Height - 1 && dir == Direction.S)) continue;
                }
                //Determine next position and check if path meets room, cluster, or other path
                pos.x = (ushort)(pos.x + DirToVec(dir).x);
                pos.y = (ushort)(pos.y + DirToVec(dir).y);
                if (GroundTilesInArea(pos) > 5) return PathGenResult.EndByClustering;
                RoomData? r = TileInRoom(pos);
                if (r != null)
                {
                    if (TileInRoom(pos, startRoom))
                    {
                        return PathGenResult.EndInStartRoom;
                    }
                    else
                    {
                        r.connected = true;
                        startRoom.connected = true;
                        return PathGenResult.EndInOtherRoom;
                    }
                }
                if (!Crossroads)
                {
                    if (InGroundIDs(Map.GetTile(pos.x, pos.y)) > -1) return PathGenResult.EndAtCrossroad;
                }
                //Place the tile
                if (ids.Count == 1)
                {
                    Map.PlaceTile(pos.x, pos.y, ids[0].id);
                }
                else
                {
                    //Determine next tile type from probability table if not in middle of a matching chain
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
                    //Extend the matching chain
                    else
                    {
                        Map.PlaceTile(pos.x, pos.y, id);
                        segLength = (ushort)(segLength + 1 >= segLimit ? 0 : segLength + 1);
                    }
                }
                if (rng.NextSingle() < PathTerminate) return PathGenResult.EndSpontaneously;
            }
        }
        /// <summary>
        /// Generates all paths in the map.
        /// </summary>
        protected void GeneratePaths()
        {
            byte exits;
            PathGenResult genResult = PathGenResult.Null;
            bool[] used = new bool[rooms.Count];
            Direction dir = 0;
            int counter = 0;
            int index = 0;
            RoomData r;
            for (int i = rooms.Count; i > 0; i--)
            {
                if (i > rooms.Count / 4)
                {
                    while (true)
                    {
                        index = rng.Next(rooms.Count);
                        if (!used[index]) break;
                    }
                }
                else 
                {
                    for (int j = 0; j < used.Length; j++)
                    {
                        if (!used[j])
                        {
                            index = j;
                            break;
                        }
                    }
                }
                r = rooms[index];
                used[index] = true;
                exits = (byte)rng.Next(MinRoomExits, MaxRoomExits + 1);
                dir = 0;
                counter = 0;
                while ((counter < exits || !r.connected) && counter < MaxRoomExits)
                {
                    while ((r.pos.x == 0 && dir == Direction.W) || (r.pos.y == 0 && dir == Direction.N) || (r.pos.x + r.size.x >= Map.Width - 1 && dir == Direction.E) || (r.pos.y + r.size.y >= Map.Height - 1 && dir == Direction.S))
                    {
                        dir = (Direction)(short)(rng.Next(4) * 90);
                    }
                    switch (dir)
                    {
                        case Direction.N:
                            genResult = CarvePath(new() { x = (ushort)(r.pos.x + rng.Next(r.size.x)), y = r.pos.y }, dir, r);
                            break;
                        case Direction.S:
                            genResult = CarvePath(new() { x = (ushort)(r.pos.x + rng.Next(r.size.x)), y = (ushort)(r.pos.y + r.size.y) }, dir, r);
                            break;
                        case Direction.W:
                            genResult = CarvePath(new() { x = r.pos.x, y = (ushort)(r.pos.y + rng.Next(r.size.y)) }, dir, r);
                            break;
                        case Direction.E:
                            genResult = CarvePath(new() { x = (ushort)(r.pos.x + r.size.x), y = (ushort)(r.pos.y + rng.Next(r.size.y)) }, dir, r);
                            break;
                    }
                    counter++;
                }
            }
        }
        /// <summary>
        /// Generates a singular pool using the specified tile ID.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="options">The settings for the tile ID to be used.</param>
        protected void FillPool(ushort x, ushort y, PoolOptions options)
        {
            try
            {
                byte tile = Map.GetTile(x, y);
                int index = InPoolIDs(tile);
                if (tile == WallID || (index > -1 && PoolIDs[index].priority > options.priority)) Map.PlaceTile(x, y, options.id);
                else return;
                if (x > 0 && rng.NextSingle() < options.spread) FillPool((ushort)(x - 1), y, options);
                if (y > 0 && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y - 1), options);
                if (x < Map.Width - 1 && rng.NextSingle() < options.spread) FillPool((ushort)(x + 1), y, options);
                if (y < Map.Height - 1 && rng.NextSingle() < options.spread) FillPool(x, (ushort)(y + 1), options);
            }
            catch (StackOverflowException)
            {
                return;
            }
        }
        /// <summary>
        /// Generate all pools in the map.
        /// </summary>
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
            ushort counter;
            for (uint i = 0; i < (uint)(MapChunks.x * MapChunks.y * PoolsPerChunk); i++)
            {
                counter = 0;
                do
                {
                    counter++;
                    pos.x = (ushort)rng.Next(Map.Width);
                    pos.y = (ushort)rng.Next(Map.Height);
                } while (Map.GetTile(pos.x, pos.y) != WallID && counter <= loopAttempts);
                rand = rng.NextSingle() * denom;
                for (index = 0; index < prob.Count; index++)
                {
                    if (rand < prob[index]) break;
                }
                FillPool(pos.x, pos.y, PoolIDs[index]);
            }
        }
        /// <summary>
        /// Converts <see cref="Direction"/> to <see cref="Vec2D_8"/>
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        protected static Vec2D_8 DirToVec(Direction dir)
        {
            return dir switch
            {
                Direction.N => new() { x = 0, y = -1 },
                Direction.S => new() { x = 0, y = 1 },
                Direction.W => new() { x = -1, y = 0 },
                Direction.E => new() { x = 1, y = 0 },
                _ => new() { x = 0, y = 0 },
            };
        }
        /// <summary>
        /// Checks if a rooms with the specified position and size would overlap with any exsisting rooms.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <remarks>The <see cref="TouchRooms"/> option changes the behavior of this method.</remarks>
        protected bool CheckRoomOverlap(Point2D_16 pos, Point2D_16 size)
        {
            foreach (RoomData r in rooms)
            {
                if (TouchRooms)
                {
                    return r.pos.x - size.x < pos.x && pos.x < r.pos.x + r.size.x &&
                           r.pos.y - size.y < pos.y && pos.y < r.pos.y + r.size.y;
                }
                else
                {
                    return r.pos.x - size.x <= pos.x && pos.x <= r.pos.x + r.size.x &&
                           r.pos.y - size.y <= pos.y && pos.y <= r.pos.y + r.size.y;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the tile at the specified position is in any room within the map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The room data of the room the tile was found in (null if none).</returns>
        /// <remarks>Works by checking if there are two adjacent ground tiles with a diagonal ground tile in between.</remarks>
        protected RoomData? TileInRoom(Point2D_16 pos)
        {
            foreach (RoomData r in rooms)
            {
                if (TileInRoom(pos, r)) return r;
            }
            return null;
        }
        /// <summary>
        /// Checks if the tile at the specified position is in the specified room.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="r">The data of the room to check.</param>
        /// <returns></returns>
        protected bool TileInRoom(Point2D_16 pos, RoomData r)
        {
            return pos.x >= r.pos.x && pos.x < r.pos.x + r.size.x && pos.y >= r.pos.y && pos.y < r.pos.y + r.size.y;
        }
        /// <summary>
        /// Counts the amount of ground tiles adjacent and diagonal to the tile in the specified position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected byte GroundTilesInArea(Point2D_16 pos)
        {
            //if (InGroundIDs(Map.GetTile(pos.x, pos.y)) == -1) return false;
            byte?[,] area = Map.GetArea(pos.x, pos.y);
            byte result = 0;
            foreach (byte? b in area)
            {
                if (b != null && InGroundIDs(b ?? WallID) > -1) result++;
            }
            return result;
        }
    }
    /// <summary>
    /// Represents a 2D point using two <see cref="ushort"/> values.
    /// </summary>
    /// <remarks>
    /// Can also be used for other purposes (e.g. sizes).
    /// </remarks>
    public class Point2D_16
    {
        public ushort x { get; set; }
        public ushort y { get; set; }
    }
    /// <summary>
    /// Represents a 2D vector using two <see cref="sbyte"/> values.
    /// </summary>
    /// <remarks>
    /// Can also be used for other purposes (e.g. coordinates & sizes).
    /// </remarks>
    public class Vec2D_8
    {
        public sbyte x { get; set; }
        public sbyte y { get; set; }
    }
    /// <summary>
    /// Return codes for path generation. Refer to <see cref="Generator.CarvePath(Point2D_16, Direction)"/>.
    /// </summary>
    public enum PathGenResult : byte
    {
        Null = 0,
        CanNotGenerate = 1,
        EndInOtherRoom = 2,
        EndInStartRoom = 3,
        EndAtBoundary = 4,
        EndAtCrossroad = 5,
        EndSpontaneously = 6,
        EndByClustering = 7
    }
    public class TileOptions
    {
        /// <summary>
        /// The ID of the tile type.
        /// </summary>
        public byte id { get; set; }
        /// <summary>
        /// The spawn rate of this type of tile.
        /// </summary>
        /// <remarks>
        /// This is not a 0 to 1 probability. Instead, it is based on the sum of all probabilities of other tile types.
        /// </remarks>
        public float spawnRate { get => spawnRateF; set => spawnRateF = value; }
        /// <summary>
        /// A one to several word description of the tile type.
        /// </summary>
        public string tag { get => tagF; set => tagF = value; }

        //Fields for the properties above
        protected string tagF = "Tile";
        private float spawnRateF = 1;
    }
    /// <summary>
    /// Extension of <see cref="TileOptions"/> for liquid tiles.
    /// </summary>
    public class PoolOptions : TileOptions
    {
        /// <summary>
        /// Probability that a tile of this type spreads to an adjacent tile suring generation.
        /// </summary>
        public float spread { get; set; }
        /// <summary>
        /// Maximum tiles each pool of this type can spread to.
        /// </summary>
        public uint maxPoolSize { get; set; }
        /// <summary>
        /// The priority of this type during pool generation compared to other types.
        /// </summary>
        /// <remarks>Lower number means higher priority. Tiles with higher priority spread over tiles with lower priority.</remarks>
        public byte priority { get; set; }
    }
    /// <summary>
    /// Extension of <see cref="TileOptions"/> for ground tiles.
    /// </summary>
    public class GroundOptions : TileOptions
    {
        /// <summary>
        /// Generate tiles of this type in rooms.
        /// </summary>
        public bool inRooms { get => inRoomsF; set => inRoomsF = value; }
        protected bool inRoomsF = true;
        /// <summary>
        /// Generate tiles of this type in rectangular patches.
        /// </summary>
        public bool patches { get => patchesF; set => patchesF = value; }
        protected bool patchesF = false;
        /// <summary>
        /// Numbers of patches to generate per room.
        /// </summary>
        /// <remarks>Only applies when <see cref="patches"/><c> == true</c>.</remarks>
        public float patchesPerRoom { get; set; }
        /// <summary>
        /// Minimum size of each patch.
        /// </summary>
        /// <remarks>Only applies when <see cref="patches"/><c> == true</c>.</remarks>
        public Point2D_16 minPatchSize { get => mnps; set => mnps = value; }
        protected Point2D_16 mnps = new() { x = 1, y = 1 };
        /// <summary>
        /// Maximum size of each patch.
        /// </summary>
        /// <remarks>Only applies when <see cref="patches"/><c> == true</c>.</remarks>
        public Point2D_16 maxPatchSize { get => mxps; set => mxps = value; }
        protected Point2D_16 mxps = new() { x = 1, y = 1 };
        /// <summary>
        /// Generate tiles of this type in pathways.
        /// </summary>
        public bool inPaths { get => inPathsF; set => inPathsF = value; }
        protected bool inPathsF = true;
        /// <summary>
        /// Generate tiles of this type in segment within pathways.
        /// </summary>
        public bool segments { get; set; }
        /// <summary>
        /// Minimum length of each segment.
        /// </summary>
        /// <remarks>Only applies when <see cref="segments"/><c> == true</c>.</remarks>
        public ushort minSegmentLength { get; set; }
        /// <summary>
        /// Maximum length of each segment.
        /// </summary>
        /// <remarks>Only applies when <see cref="segments"/><c> == true</c>.</remarks>
        public ushort maxSegmentLength { get; set; }
    }
    /// <summary>
    /// Contains data related to a room.
    /// </summary>
    public class RoomData
    {
        /// <summary>
        /// The coordinates of the top-left corner.
        /// </summary>
        public Point2D_16 pos = new Point2D_16{ x = 0, y = 0 };
        public Point2D_16 size = new Point2D_16{ x = 0, y = 0 };
        /// <summary>
        /// Whether the room is connected to any other room.
        /// </summary>
        public bool connected;
    }
    /// <remarks>Each value corresponds with a short representing the compass angle (North = 0) of the direction in degrees.</remarks>
    public enum Direction : short
    {
        N = 0,
        NE = 45,
        E = 90,
        SE = 135,
        S = 180,
        SW = 225,
        W = 270,
        NW = 315,
    }
}
