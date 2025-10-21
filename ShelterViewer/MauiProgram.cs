using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using ShelterViewer.Shared.Services.UserPreferences;
using ShelterViewer.Shared.Services;
using ShelterViewer.Shared.Services.VaultServices;
using ShelterViewer.Services;

#if WINDOWS
using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
#endif

namespace ShelterViewer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if WINDOWS && DEBUG
    try     {
        var env = CoreWebView2Environment.GetAvailableBrowserVersionString();
        System.Diagnostics.Debug.WriteLine($"WebView2 Runtime Version: {env}");
        Msg.Info($"WebView2 Runtime Version: {env}");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error retrieving WebView2 Runtime Version: {ex.Message}");
    }
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddMudServices();

        builder.Services.AddSingleton<IRoomTypeLoader, MauiRoomTypeLoader>();
        builder.Services.AddSingleton<VaultService>();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        builder.Services.AddScoped<LayoutService>();

        return builder.Build();
    }
}


static class Msg
{
    [DllImport("User32.dll", CharSet= CharSet.Unicode, SetLastError = false)]
    private static extern int MessageBoxW(nint hWnd, string text, string caption, uint type);

    public static void Info(string text, string caption = "WebView2 Probe")
    {
        MessageBoxW(0, text, caption, 0x00000040);
    }
}