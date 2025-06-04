using Avalonia.Controls;
using DunGenLib;

namespace DunGenApp
{
    public partial class MainWindow : Window
    {
        private Map map = new(1, 1);
        private Generator gen;
        public MainWindow()
        {
            InitializeComponent();
            gen = new Generator { map = map };
        }
    }
}