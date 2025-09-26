using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using DunGenLib;
using MsBox.Avalonia;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace DunGenApp
{
    public partial class MainWindow : Window
    {
        private Map map = new(128, 128);
        private DynamicGenerator gen { get; set; }
        private TextureInfo textures;
        public MainWindow()
        {
            gen = new DynamicGenerator(map);
            /*gen = new DynamicGenerator(map)
            {
                LoopAttempts = 10,
                MergeRooms = false,
                TouchRooms = false,
                MapChunks = new() { x = 8, y = 8 },
                MinRoomsPerChunk = 1,
                MaxRoomsPerChunk = 1,
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
            textures = new() { gen = gen };
            InitializeComponent();
            InitializeUI();
        }
        public void InitializeUI()
        {
            gen.PropertyChanged += (source, ev) => MapStatus.Text = "Settings changed";
            gen.ObservableGroundIDs.CollectionChanged += (source, ev) => MapStatus.Text = "Settings changed";
            gen.ObservablePoolIDs.CollectionChanged += (source, ev) => MapStatus.Text = "Settings changed";
            GenSettings.DataContext = gen;
        }
        public MainWindow(DynamicGenerator g)
        {
            gen = g;
            gen.UpdateIDs();
            textures = new() { gen = gen };
            InitializeComponent();
            InitializeUI();
        }
        private async void SaveOptions(object source, RoutedEventArgs e)
        {
            var file = await TopLevel.GetTopLevel(this)?.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Generator Preset",
                FileTypeChoices = new[] { new FilePickerFileType("JSON document") { Patterns = ["*.json"], MimeTypes = new[] { "application/json" } } }
            });
            if (file != null)
            {
                File.WriteAllText(file.Path.AbsolutePath, JsonSerializer.Serialize(gen, gen.GetType(), new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        private async void LoadOptions(object source, RoutedEventArgs e)
        {
            var files = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Load Generator Preset",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("JSON document") { Patterns = ["*.json"], MimeTypes = new[] { "application/json" } } }
            });
            if (files.Count > 0)
            {
                MainWindow w = new(JsonSerializer.Deserialize<DynamicGenerator>(File.ReadAllText(files[0].Path.AbsolutePath)) ?? new DynamicGenerator { Map = map });
                w.Show();
                Close();
            }
        }
        private void ResetOptions(object? source, RoutedEventArgs e)
        {
            map = new(128, 128);
            gen = new DynamicGenerator { Map = map };
            gen.UpdateIDs();
            textures = new() { gen = gen };
            gen.PropertyChanged += (source, ev) => MapStatus.Text = "Settings changed";
            gen.ObservableGroundIDs.CollectionChanged += (source, ev) => MapStatus.Text = "Settings changed";
            gen.ObservablePoolIDs.CollectionChanged += (source, ev) => MapStatus.Text = "Settings changed";
            GenSettings.DataContext = gen;
        }
        private void GenerateMap(object? source, RoutedEventArgs e)
        {
            MapStatus.Text = "Generating...";
            map.Resize(UInt16.Parse(MapWidth?.Text ?? "1"), UInt16.Parse(MapHeight?.Text ?? "1"));
            gen.GenerateMap();
            MapStatus.Text = "Map ready";
            EditMapBtn.IsEnabled = true;
        }
        private void AddGroundType(object? source, RoutedEventArgs e)
        {
            byte? newID = gen.FindVacantTileID();
            if (newID == null) MessageBoxManager.GetMessageBoxStandard("No IDs available", "The entire ID range (0 ~ 255) is occupied.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            else
            {
                gen.ObservableGroundIDs.Add(new GroundOptions { id = newID ?? 0 });
                textures.activated = false;
            }
        }
        private void RemoveGroundType(object? source, RoutedEventArgs e)
        {
            if (GroundTypes.SelectedIndex > -1 && gen.ObservableGroundIDs.Count > 1)
            {
                gen.ObservableGroundIDs.RemoveAt(GroundTypes.SelectedIndex);
                GroundTypes.SelectedIndex = -1;
            }
        }
        private void AddPoolType(object? source, RoutedEventArgs e)
        {

            byte? newID = gen.FindVacantTileID();
            if (newID == null) MessageBoxManager.GetMessageBoxStandard("No IDs available", "The entire ID range (0 ~ 255) is occupied.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
            else
            {
                gen.ObservablePoolIDs.Add(new PoolOptions { id = newID ?? 0 });
                textures.activated = false;
            }
        }
        private void RemovePoolType(object? source, RoutedEventArgs e)
        {
            if (PoolTypes.SelectedIndex > -1 && gen.ObservablePoolIDs.Count > 1)
            {
                gen.ObservablePoolIDs.RemoveAt(PoolTypes.SelectedIndex);
                PoolTypes.SelectedIndex = -1;
            }
        }
        private void EditTextures(object? source, RoutedEventArgs e)
        {
            TextureEditor w = new TextureEditor(textures);
            w.ShowDialog(this);
        }
        private void EditMap(object? source, RoutedEventArgs e)
        {
            if (textures.activated)
            {
                MapEditor w = new MapEditor(textures);
                w.Show();
            }
            else
            {
                TextureEditor w = new TextureEditor(textures, true);
            }
        }
        private void PrintMap(object? source, RoutedEventArgs e)
        {
            string disp = "";
            for (ushort y = 0; y < map.Height; y++)
            {
                for (ushort x = 0; x < map.Width; x++)
                {
                    disp += map.GetTile(x, y);
                }
                disp += "\n";
            }
            ConsoleView.Text = disp;
        }
    }
}