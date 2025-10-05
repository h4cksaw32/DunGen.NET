using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DunGenLib;

namespace DunGenApp
{
    /// <summary>
    /// A versoin of <see cref="Generator"/> that implements <see cref="INotifyPropertyChanged"/> to signal changes in each setting.
    /// </summary>
    /// <remarks>
    /// Note that <see cref="Generator.GroundIDs"/> and <see cref="Generator.PoolIDs"/> are instead mirrored by <see cref="ObservableCollection{T}"/> lists.
    /// </remarks>
    public class DynamicGenerator : Generator, INotifyPropertyChanged
    {
        public DynamicGenerator() : base() {  }
        public DynamicGenerator(Map m) : base(m) {  }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void InvokeChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public new ushort LoopAttempts
        {
            get => loopAttempts;
            set
            {
                loopAttempts = value;
                InvokeChange();
            }
        }
        public new Point2D_16 MapChunks
        {
            get => mapChunks;
            set
            {
                mapChunks = value;
                InvokeChange();
            }
        }
        public new Point2D_16 MinRoomSize
        {
            get => minRoomSize;
            set
            {
                minRoomSize = value;
                InvokeChange();
            }
        }
        public new Point2D_16 MaxRoomSize
        {
            get => maxRoomSize;
            set
            {
                maxRoomSize = value;
                InvokeChange();
            }
        }
        public new bool MergeRooms
        {
            get => mergeRooms;
            set
            {
                mergeRooms = value;
                InvokeChange();
            }
        }
        public new bool TouchRooms
        {
            get => touchRooms;
            set
            {
                touchRooms = value;
                InvokeChange();
            }
        }
        public new byte MinRoomsPerChunk
        {
            get => minRoomsPerChunk;
            set
            {
                minRoomsPerChunk = value;
                InvokeChange();
            }
        }
        public new byte MaxRoomsPerChunk
        {
            get => maxRoomsPerChunk;
            set
            {
                maxRoomsPerChunk = value;
                InvokeChange();
            }
        }
        public new byte MinRoomExits
        {
            get => minRoomExits;
            set
            {
                minRoomExits = value;
                InvokeChange();
            }
        }
        public new byte MaxRoomExits
        {
            get => maxRoomExits;
            set
            {
                maxRoomExits = value;
                InvokeChange();
            }
        }
        public new float PoolsPerChunk
        {
            get => poolsPerChunk;
            set
            {
                poolsPerChunk = value;
                InvokeChange();
            }
        }
        public new float PathBend
        {
            get => pathBend;
            set
            {
                pathBend = value;
                InvokeChange();
            }
        }
        public new float PathTerminate
        {
            get => pathTerminate;
            set
            {
                pathTerminate = value;
                InvokeChange();
            }
        }
        public new bool Crossroads
        {
            get => crossroads;
            set
            {
                crossroads = value;
                InvokeChange();
            }
        }
        public new bool EndAtBoundary
        {
            get => endAtBoundary;
            set
            {
                endAtBoundary = value;
                InvokeChange();
            }
        }
        public new byte WallID
        {
            get => wallID;
            set
            {
                wallID = value;
                InvokeChange();
            }
        }
        public ObservableCollection<GroundOptions> ObservableGroundIDs { get => observeGroundIDs; set { observeGroundIDs = value; GroundIDs = [.. value]; } }
        protected ObservableCollection<GroundOptions> observeGroundIDs = [new GroundOptions { id = 1, inPaths = true, inRooms = true, spawnRate = 1, tag = "Ground" }];
        public ObservableCollection<PoolOptions> ObservablePoolIDs { get => observePoolIDs; set { observePoolIDs = value; PoolIDs = [.. value]; } }
        protected ObservableCollection<PoolOptions> observePoolIDs = [new PoolOptions { id = 2, spawnRate = 1, spread = 0.5F, tag = "Water", maxPoolSize = 40000, priority = 0 }];
        public new byte? FindVacantTileID()
        {
            UpdateIDs();
            return base.NextVacantTileID();
        }
        public void UpdateIDs()
        {
            GroundIDs = [.. ObservableGroundIDs];
            PoolIDs = [.. ObservablePoolIDs];
        }
        public void ExposeIDs()
        {
            ObservableGroundIDs = [.. GroundIDs];
            ObservablePoolIDs = [.. PoolIDs];
        }
        public override void ValidateIDs()
        {
            UpdateIDs();
            base.ValidateIDs();
            ExposeIDs();
        }
    }
}
