using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ShelterViewer.Models;
using ShelterViewer.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ShelterViewer.Pages;

namespace ShelterViewer.Services;

public class VaultService
{
    public event Action? OnVaultChanged = null;
    
    public string VaultString { get; set; } = String.Empty;
    public VaultData? VaultData { get; private set; }
    public VaultData? VaultData2 { get; private set; }

    private IJSRuntime JS;
    private dynamic? _vaultData = null;
    private List<Dweller> _dwellers = new();
    private List<Room> _rooms = new();
    private List<IItem> _items = new();


    public int DwellerCount
    {
        get
        {
            return VaultData!.dwellers.dwellers.Count();
        }
    }

    public int RoomCount
    {
        get
        {
            return VaultData!.Vault.rooms.Count();
        }
    }

    public int LunchBoxes
    {
        get
        {
            return VaultData!.Vault.LunchBoxesByType.Count(x => x == 0);
        }
    }

    public int MrHandy
    {
        get
        {
            return VaultData!.Vault.LunchBoxesByType.Count(x => x == 1);
        }
    }

    public int PetCarriers
    {
        get
        {
            return VaultData!.Vault.LunchBoxesByType.Count(x => x == 2);
        }
    }

    public int StarterPacks
    {
        get
        {
            return VaultData!.Vault.LunchBoxesByType.Count(x => x == 3);
        }
    }

    public List<Dweller> Dwellers
    {
        get
        {
            return VaultData!.dwellers.dwellers.ToList();
        }
    }

    public List<Room> Rooms
    {
        get
        {
            return VaultData!.Vault.rooms.ToList();
        }
    }

    public List<IItem> Weapons
    {
        get
        {
            return _items.Where(i => i.type == "Weapon").ToList() ?? new List<IItem>();
        }
    }

    public List<IItem> Outfits
    {
        get
        {
            return _items.Where(i => i.type == "Outfit").ToList() ?? new List<IItem>();
        }
    }

    public List<IItem> Junk
    {
        get
        {
            return _items.Where(i => i.type == "Junk").ToList() ?? new List<IItem>();
        }
    }

    public List<IItem> Pets
    {
        get
        {
            return _items.Where(i => i.type == "Pet").ToList() ?? new List<IItem>();
        }
    }

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

    public VaultService(IJSRuntime jsRuntime)
    {
        JS = jsRuntime;
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
            NotifyPropertyChanged();
 
        } 
        catch (Exception ex)
        {
            VaultString = String.Empty;
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
    public void CloseVault()
    {
        VaultString = String.Empty;
        _vaultData = null;
        NotifyPropertyChanged();
    }
    public bool IsVaultEmpty()
    {
        return VaultString == String.Empty;
    }

    public Room? GetRoom(int roomNumber)
    {
        return Rooms.FirstOrDefault(r => r.deserializeID == roomNumber);
    }

    private List<IItem> GetItems()
    {
        List<IItem> items = new();
        //List<Dweller> dwellers = GetDwellers();
        // Items are located in multiple places. 

        _dwellers.Select(dweller => dweller.equipedOutfit).Where(o => o.id != "jumpsuit").ToList().ForEach(item => items.Add(item));
        _dwellers.Select(dweller => dweller.equipedWeapon).Where(w => w.id != "Fist").ToList().ForEach(item => items.Add(item));
        _dwellers.Select(dwellers => dwellers.equippedPet).Where(p => p != null).ToList().ForEach(item => items.Add(item!));

        var itemsList = (_vaultData?.vault.inventory?.items as IEnumerable<dynamic>) ?? new List<dynamic>();
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
        if(JS != null)
            JS.InvokeVoidAsync("console.log", message);
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        OnVaultChanged?.Invoke();
    }


    public record VaultLevel(float Level, float Max);
    public record VaultLevels(VaultLevel Food, VaultLevel Energy, VaultLevel Water);
    
}
