using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGen.NET
{
    public class Generator
    {
        public required Grid grid;
        private readonly Random rng = new();
        public required Value2D<byte> TopLeftBound;
        public required Value2D<byte> BottomRightBound;
        public Value2D<byte> MapSegments = new Value2D<byte>{ x = 5, y = 5 };

        public Value2D<byte> MinRoomSize = new Value2D<byte> { x = 3, y = 3 };
        public Value2D<byte> MaxRoomSize = new Value2D<byte> { x = 16, y = 16 };
        public bool OverlapRooms = false;
        public byte RoomsPerSegment = 1;
        public byte MinRoomExits = 1;
        public byte MaxRoomExits = 4;

        public float PoolJaggedness = 0.5F;
        public bool OverlapPools = false;
        public byte PoolsPerSegment = 1;

        public float PathBend = 0.15F;
        public float PathTerminate = 0.05F;
        public bool Crossroads = true;
        public bool EndAtBoundary = true;

        public byte WallID = 0;
        public required PoolOptions[] PoolIDs;
        public required GroundOptions[] GroundIDs;
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
    public struct Value2D<T>
    {
        public T x;
        public T y;
    }
}
