using ShelterViewer.Shared.Models.Data;
using ShelterViewer.Shared.Services.VaultServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShelterViewer.Services;
public class MauiRoomTypeLoader : IRoomTypeLoader
{
    public async Task<RoomTypeRoot?> LoadRoomTypesAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("rooms.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var rooms = JsonSerializer.Deserialize<RoomTypeRoot>(json);
            return rooms;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to load room types", ex);
            return null;
        }
    }
}
