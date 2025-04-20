using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using HallCalc;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("jsinterop.js", "../jsinterop.js");
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}