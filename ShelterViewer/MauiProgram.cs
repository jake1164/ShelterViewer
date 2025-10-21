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
#if WINDOWS
        builder.Services.AddScoped<IVaultFileService, WindowsVaultFileService>();
#elif ANDROID
        builder.Services.AddScoped<IVaultFileService, FilePickerVaultFileService>();
#elif IOS
        builder.Services.AddScoped<IVaultFileService, FilePickerVaultFileService>();
#elif MACCATALYST
        builder.Services.AddScoped<IVaultFileService, FilePickerVaultFileService>();
#else
        builder.Services.AddScoped<IVaultFileService, BrowserVaultFileService>();
#endif
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        builder.Services.AddScoped<LayoutService>();

        return builder.Build();
    }
}