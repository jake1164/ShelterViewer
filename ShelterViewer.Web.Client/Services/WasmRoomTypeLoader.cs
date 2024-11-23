using ShelterViewer.Shared.Models.Data;
using ShelterViewer.Shared.Services.VaultServices;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace ShelterViewer.Web.Client.Services;

public class WasmRoomTypeLoader : IRoomTypeLoader
{
    private readonly HttpClient _httpClient;

    public WasmRoomTypeLoader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RoomTypeRoot?> LoadRoomTypesAsync()
    {
        try
        {
            var rooms = await _httpClient.GetFromJsonAsync<RoomTypeRoot>("data/rooms.json");
            return rooms;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to load room types", ex);
            return null;
        }
    }
}
