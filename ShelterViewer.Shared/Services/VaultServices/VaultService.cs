using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using ShelterViewer.Shared.Models;
using ShelterViewer.Shared.Utility;

namespace ShelterViewer.Shared.Services.VaultServices;

public class VaultService
{
    public event Action? OnVaultChanged = null;

    public string VaultString { get; set; } = string.Empty;
    public VaultData? VaultData { get; private set; }

    private IJSRuntime JS;
    private readonly IRoomTypeLoader _roomTypeLoader;
    private dynamic? _vaultData = null;
    private List<IItem> _items = new();
    private List<Models.Data.RoomType> _roomTypes = new();

    private Task? _loadRoomTypesTask;

    public int DwellerCount => VaultData?.dwellers.dwellers.Count() ?? 0;
    public int RoomCount => VaultData?.Vault.rooms.Count() ?? 0;
    public List<Dweller> DwellerList => VaultData!.dwellers.dwellers.ToList();
    public Dictionary<int, Dweller> Dwellers => VaultData!.dwellers.dwellers.ToDictionary(d => d.serializeId);
    public List<Room> Rooms => VaultData!.Vault.rooms.ToList();
    public int GetLunchBoxesCountByType(int type) => VaultData!.Vault.LunchBoxesByType.Count(x => x == type);
    public int LunchBoxes => GetLunchBoxesCountByType(0);
    public int MrHandy => GetLunchBoxesCountByType(1);
    public int PetCarriers => GetLunchBoxesCountByType(2);
    public int StarterPacks => GetLunchBoxesCountByType(3);
    public List<IItem> GetItemByType(string type) => _items.Where(i => i.type == type).ToList();
    public List<IItem> Weapons => GetItemByType("Weapon");
    public List<IItem> Outfits => GetItemByType("Outfit");
    public List<IItem> Junk => GetItemByType("Junk");
    public List<IItem> Pets => GetItemByType("Pet");

    public VaultLevels VaultResources
    {
        get
        {
            float f = _vaultData?.vault.storage.resources.Food ?? 0;
            float e = _vaultData?.vault.storage.resources.Energy ?? 0;
            float w = _vaultData?.vault.storage.resources.Water ?? 0;
            var food = new VaultLevel(f, 0);
            var energy = new VaultLevel(e, 0);
            var water = new VaultLevel(w, 0);

            return new VaultLevels(food, energy, water);
        }
    }

    public VaultService(IJSRuntime jsRuntime, IRoomTypeLoader roomTypeLoader)
    {
        JS = jsRuntime;
        _roomTypeLoader = roomTypeLoader;
        _loadRoomTypesTask = LoadRoomTypesAsync();
    }

    /// <summary>
    /// Loads the RoomTypes from the .json file based on the app being run
    /// ie blazor (http get) vs maui (stream reader).
    /// </summary>
    /// <returns>task when complete </returns>
    public async Task LoadRoomTypesAsync()
    {
        try
        {
            var rooms = await _roomTypeLoader.LoadRoomTypesAsync();

            if (rooms != null)
            {
                _roomTypes = rooms.Rooms;
                Console.WriteLine($"Loaded {rooms?.Rooms.Count} room types");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to load room types", ex);
        }
    }

    public void InitializeVault(string vaultJsonString)
    {
        try
        {
            var settings = new IntJsonConverter();
            VaultString = vaultJsonString;
            _vaultData = JsonConvert.DeserializeObject<dynamic>(VaultString, settings);

            VaultData = System.Text.Json.JsonSerializer.Deserialize<VaultData>(VaultString, new JsonSerializerOptions());
            VaultData!.dwellers.dwellers = VaultData!.dwellers.dwellers.ToList().OrderBy(x => x.serializeId).ToArray(); // Order by ID initially.

            _items = GetItems(); // Items are spread across users.
            ProcessDwellers(); // Do this on the first click of dwellers?
            ProcessRooms(); // Do this on the first click of rooms?
            NotifyPropertyChanged();

        }
        catch (Exception ex)
        {
            VaultString = string.Empty;
            Log("Unable to convert vault string to JSON Object: " + ex.Message);
        }
    }

    private void ProcessDwellers()
    {
        // foreach dweller in vaultdata.dwellers find the mother and father and add them to the dweller object and then add the dweller to the mother and father as a child.
        foreach (var dweller in VaultData!.dwellers.dwellers)
        {
            dweller.Mother = VaultData!.dwellers.dwellers.FirstOrDefault(d => d.serializeId == dweller.relations.ascendants[1]);
            dweller.Father = VaultData!.dwellers.dwellers.FirstOrDefault(d => d.serializeId == dweller.relations.ascendants[0]);
            if (dweller.Mother != null)
            {
                dweller.Mother.Children.Add(dweller);
            }
            if (dweller.Father != null)
            {
                dweller.Father.Children.Add(dweller);
            }
        }
    }

    private void ProcessRooms()
    {
        foreach (var room in VaultData!.Vault.rooms)
        {
            // DwellerIds in the room are what are used for displaying dwellers not the savedRoom.
            foreach(var dwellerId in room.DwellerIds)
            {
                if(Dwellers.ContainsKey(dwellerId))
                    Dwellers[dwellerId].AssignedRoom = room.deserializeID;
            }

            // Populate the room types with the room data.
            var roomType = _roomTypes.FirstOrDefault(r => r.Type == room.type && r.Level == room.level);
            if (roomType != null)
            {
                room.Name = roomType.Name;
                // Map rest of the roomtype to room.
                room.Trait = roomType.Trait;
                room.OutputType = roomType.OutputType;

                int i = room.mergeLevel - 1;
                // Mapped based on level
                // Room size can have a single value OR one value / room size.  
                if (roomType.Size != null && roomType.Size.Length == 1)
                    room.Size = roomType.Size[0];
                else if(roomType.Size != null && roomType.Size.Length > 1)
                    room.Size = roomType.Size[i];
                
                if(roomType.Output != null)
                    room.Output = roomType.Output[i];

                if(roomType.Storage != null)
                    room.StorageCapacity = roomType.Storage[i];

                if (roomType.Capacity != null)
                    room.DwellerCapacity = roomType.Capacity[i];

                if(roomType.PowerPerMin != null)
                    room.PowerPerMin = roomType.PowerPerMin[i];

                //room.Storage = roomType.Storage;
                // TODO: I do not think this includes all dwellers in room?
                room.Dwellers = VaultData!.dwellers.dwellers.Where(d => room.DwellerIds.Contains(d.serializeId)).ToArray();
            }
            else
            {
                Console.WriteLine($"Unable to find room type: {room.type}");
            }
        }
    }

    public void CloseVault()
    {
        VaultString = string.Empty;
        _vaultData = null;
        NotifyPropertyChanged();
    }
    public bool IsVaultEmpty()
    {
        return VaultString == string.Empty;
    }

    public Room? GetRoom(int roomNumber)
    {
        return Rooms.FirstOrDefault(r => r.deserializeID == roomNumber);
    }

    private List<IItem> GetItems()
    {
        List<IItem> items = new();
        // Items are located in multiple places. 

        DwellerList.Select(dweller => dweller.equipedOutfit).Where(o => o.id != "jumpsuit").ToList().ForEach(item => items.Add(item));
        DwellerList.Select(dweller => dweller.equipedWeapon).Where(w => w.id != "Fist").ToList().ForEach(item => items.Add(item));

        var dwellerPet = DwellerList.Select(dweller => dweller.equippedPet).ToList();

        DwellerList.Select(dwellers => dwellers.equippedPet).Where(p => p != null).ToList().ForEach(item => items.Add(item!));

        var itemsList = _vaultData?.vault.inventory?.items as IEnumerable<dynamic> ?? new List<dynamic>();
        foreach (var item in itemsList)
        {
            try
            {
                switch (item["type"].ToString())
                {
                    case "Outfit":
                        items.Add(JsonConvert.DeserializeObject<Outfit>(item.ToString()));
                        break;
                    case "Weapon":
                        items.Add(JsonConvert.DeserializeObject<Weapon>(item.ToString()));
                        break;
                    case "Junk":
                        items.Add(JsonConvert.DeserializeObject<Item>(item.ToString()));
                        break;
                    case "Pet":
                        items.Add(JsonConvert.DeserializeObject<Pet>(item.ToString()));
                        break;
                    default:
                        Log("Unknown Item Type: " + item.type);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log("Unable to convert items string to JSON Object: " + ex.Message);
            }
        }
        return items;
    }
    private void Log(params object?[]? message)
    {
        if (JS != null)
            JS.InvokeVoidAsync("console.log", message);
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        OnVaultChanged?.Invoke();
    }

    public record VaultLevel(float Level, float Max);
    public record VaultLevels(VaultLevel Food, VaultLevel Energy, VaultLevel Water);

}
