using Avalonia;

namespace Clock2;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Keep GPU rendering but cap Skia's GPU resource cache; a 210px clock needs very little.
            .With(new SkiaOptions { MaxGpuResourceSizeBytes = 4 * 1024 * 1024 });
    }
}
