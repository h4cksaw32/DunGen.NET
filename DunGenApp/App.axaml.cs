using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using System.Reflection;

namespace DunGenApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
        static Assembly AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            string assembliesDir = "dir";
            Assembly asm = Assembly.LoadFrom(Path.Combine(assembliesDir, args.Name + ".dll"));
            return asm;
        }
    }
}