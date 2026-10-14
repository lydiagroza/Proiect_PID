using System;
using Avalonia;

namespace Framework
{
    internal static class Program
    {
        // Avalonia is not initialized before Main runs, so no Avalonia code can run before StartWithClassicDesktopLifetime.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Also used by the XAML previewer of the IDE.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }
    }
}
