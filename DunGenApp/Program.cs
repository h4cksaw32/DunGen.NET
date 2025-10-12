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
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        private static Assembly AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            string altDir = Path.Combine("lib", args.Name + ".dll");
            Assembly asm = Assembly.LoadFile(File.Exists(altDir) ? altDir : args.Name + ".dll");
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
