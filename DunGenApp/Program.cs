using Avalonia;
using System;
using System.IO;
using System.Reflection;

namespace DunGenApp
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            Start(args);
        }
        internal static void Start(string[] args) =>
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        private static Assembly AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            string defDir = args.RequestingAssembly.GetName().Name + ".dll";
            string altDir = Path.Combine("lib", args.RequestingAssembly.GetName().Name + ".dll");
            Assembly asm = Assembly.LoadFrom(File.Exists(defDir) ? defDir : altDir);
            return asm;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
