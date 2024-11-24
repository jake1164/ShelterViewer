using ShelterViewer.Web.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ShelterViewer.Shared.Services;
using Blazored.LocalStorage;
using ShelterViewer.Shared.Services.UserPreferences;
using ShelterViewer.Shared.Services.VaultServices;
using ShelterViewer.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<IRoomTypeLoader, WasmRoomTypeLoader>();
builder.Services.AddScoped<VaultService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<LayoutService>();

await builder.Build().RunAsync();
