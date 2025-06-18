using Avalonia.Controls;
using Avalonia.Interactivity;
using DunGenLib;
using System;

namespace DunGenApp
{
    public partial class MainWindow : Window
    {
        private Map map = new(1, 1);
        private DynamicGenerator gen {  get; set; }
        public MainWindow()
        {
            InitializeComponent();
            gen = new DynamicGenerator { Map = map, GroundIDs = [new GroundOptions { id = 0, inPaths = true, inRooms = true, spawnRate = 1 }], PoolIDs = [new PoolOptions { id = 2, spawnRate = 1, spread = 0.5F }], WallID = 1 };
            GenSettings.DataContext = gen;
        }
        private void GenerateMap(object? source, RoutedEventArgs e)
        {
            map.Resize(UInt16.Parse(MapWidth?.Text ?? "1"), UInt16.Parse(MapHeight?.Text ?? "1"));
            gen.GenerateMap();
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