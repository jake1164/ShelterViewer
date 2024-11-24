using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using ShelterViewer.Shared.Services.UserPreferences;
using ShelterViewer.Shared.Services;
using ShelterViewer.Shared.Services.VaultServices;
using ShelterViewer.Services;

namespace ShelterViewer
{
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
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
            builder.Services.AddScoped<LayoutService>();

            return builder.Build();
        }
    }
}
