using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using DunGenLib;
using MsBox.Avalonia;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace DunGenApp
{
    public partial class MainWindow : Window
    {
        private Map map = new(1, 1);
        private DynamicGenerator gen { get; set; }
        public MainWindow()
        {
            gen = new DynamicGenerator { Map = map };
            /*gen = new DynamicGenerator
            {
                Map = map,
                LoopAttempts = 10,
                MergeRooms = false,
                TouchRooms = false,
                MapChunks = new() { x = 8, y = 8 },
                RoomsPerChunk = 1.0F,
                PoolsPerChunk = 0.7F,
                MinRoomSize = new() { x = 4, y = 4 },
                PathTerminate = 0F,
                EndAtBoundary = false,
                WallID = 1,
                ObservablePoolIDs = [
                    new PoolOptions{id = 2, spawnRate = 0.6F, spread = 0.6F},
                    new PoolOptions{id = 5, spawnRate = 0.4F, spread = 0.4F},
                ],
                ObservableGroundIDs = [
                    new GroundOptions{id = 0, spawnRate = 1.0F, inRooms = true, inPaths = false},
                    new GroundOptions{id = 6, spawnRate = 0.2F, inRooms = false, patches = false, inPaths = true, segments = false, minSegmentLength = 5, maxSegmentLength = 20},
                    new GroundOptions{id = 8, spawnRate = 0.5F, inRooms = true, patches = false, inPaths = true, segments = true, minSegmentLength = 5, maxSegmentLength = 10},
                    new GroundOptions{id = 9, spawnRate = 0.3F, inRooms = false, patches = false, inPaths = true, segments = false, minSegmentLength = 5, maxSegmentLength = 20},
                    new GroundOptions{id = 3, spawnRate = 0.5F, inRooms = true, patches = false, inPaths = false},
                    new GroundOptions{id = 7, spawnRate = 0.5F, inRooms = true, patches = true, inPaths = false, patchesPerRoom = 0.5F, minPatchSize = new() { x = 2, y = 2 }, maxPatchSize = new() { x = 6, y = 6 }},
                    new GroundOptions{id = 4, spawnRate = 0.5F, inRooms = true, patches = true, inPaths = false, patchesPerRoom = 0.5F, minPatchSize = new() { x = 3, y = 3 }, maxPatchSize = new() { x = 8, y = 8 }},
                ]
            };*/
            gen.UpdateIDs();
            InitializeComponent();
            gen.PropertyChanged += (source, ev) => MapStatus.Text = "Settings changed";
            GenSettings.DataContext = gen;
        }
        private void GenerateMap(object? source, RoutedEventArgs e)
        {
            MapStatus.Text = "Generating...";
            map.Resize(UInt16.Parse(MapWidth?.Text ?? "1"), UInt16.Parse(MapHeight?.Text ?? "1"));
            gen.UpdateIDs();
            gen.GenerateMap();
            MapStatus.Text = "Map ready";
        }
        private void AddGroundType(object? source, RoutedEventArgs e)
        {
            byte? newID = FindLatestTileID();
            if (newID == null) MessageBoxManager.GetMessageBoxStandard("No IDs available", "The entire ID range (0 ~ 255) is occupied.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            else gen.ObservableGroundIDs.Add(new GroundOptions { id = newID ?? 0 });
        }
        private void RemoveGroundType(object? source, RoutedEventArgs e)
        {
            if (GroundTypes.SelectedIndex > -1)
            {
                gen.ObservableGroundIDs.RemoveAt(GroundTypes.SelectedIndex);
                GroundTypes.SelectedIndex = -1;
            }
        }
        private byte? FindLatestTileID()
        {
            gen.UpdateIDs();
            byte? id = 0;
            bool[] available = new bool[256];
            Array.Fill<bool>(available, false);
            available[gen.WallID] = true;
            foreach (GroundOptions g in gen.GroundIDs)
            {
                available[g.id] = true;
            }
            foreach (PoolOptions p in gen.PoolIDs)
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
        private void AddPoolType(object? source, RoutedEventArgs e)
        {

            byte? newID = FindLatestTileID();
            if (newID == null) MessageBoxManager.GetMessageBoxStandard("No IDs available", "The entire ID range (0 ~ 255) is occupied.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            else gen.ObservablePoolIDs.Add(new PoolOptions { id = newID ?? 0 });
        }
        private void RemovePoolType(object? source, RoutedEventArgs e)
        {
            if (PoolTypes.SelectedIndex > -1)
            {
                gen.ObservablePoolIDs.RemoveAt(PoolTypes.SelectedIndex);
                PoolTypes.SelectedIndex = -1;
            }
        }
        private void PrintMap(object? source, RoutedEventArgs e)
        {
            string disp = "";
            for (ushort y = 0; y < map.height; y++)
            {
                for (ushort x = 0; x < map.width; x++)
                {
                    disp += map.GetTile(x, y);
                }
                disp += "\n";
            }
            ConsoleView.Text = disp;
        }
    }
}