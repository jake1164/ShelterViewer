using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

    private IJSRuntime JS;
    private string _vaultString = String.Empty;

    private dynamic? _vaultData = null;
    private List<Dweller> _dwellers = new();
    private List<Room> _rooms = new();
    private List<IItem> _items = new();
    

    public string Name 
    { 
        get 
        { 
            return _vaultData?.vault.VaultName ?? String.Empty; 
        } 
    }

    public int DwellerCount
    {
        get
        {
            return _vaultData?.dwellers.dwellers.Count ?? 0;
        }
    }

    public int RoomCount
    {
        get
        {
            return _vaultData?.vault.rooms.Count ?? 0;
        }
    }

    public int Caps
    {
        get
        {
            return _vaultData?.vault.storage.resources.Nuka ?? 0;
        }
    }

    public int Quantums
    {
        get
        {
            return _vaultData?.vault.storage.resources.NukaColaQuantum ?? 0;
        }
    }

    public int LunchBoxes
    {
        get
        {
            var lunchboxes = (_vaultData?.vault.LunchBoxesByType as IEnumerable<dynamic>) ?? new List<dynamic>();
            return lunchboxes.Count(x => x == 0);
        }
    }

    public int MrHandy
    {
        get
        {
            var lunchboxes = (_vaultData?.vault.LunchBoxesByType as IEnumerable<dynamic>) ?? new List<dynamic>();
            return lunchboxes.Count(x => x == 1);
        }
    }

    public int PetCarriers
    {
        get
        {
            var lunchboxes = (_vaultData?.vault.LunchBoxesByType as IEnumerable<dynamic>) ?? new List<dynamic>();
            return lunchboxes.Count(x => x == 2);
        }
    }

    public int StarterPacks
    {
        get
        {
            var lunchboxes = (_vaultData?.vault.LunchBoxesByType as IEnumerable<dynamic>) ?? new List<dynamic>();
            return lunchboxes.Count(x => x == 3);
        }
    }

    public int StimPacks
    {
        get
        {
            return _vaultData?.vault.storage.resources.StimPack ?? 0;
        }
    }

    public int RadAways
    {
        get
        {
            return _vaultData?.vault.storage.resources.RadAway ?? 0;
        }
    }

    public List<Dweller> Dwellers
    {
        get
        {
            return _dwellers;
        }
    }

    public List<Room> Rooms
    {
        get
        {
            return _rooms;
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


    public VaultService(IJSRuntime jsRuntime)
    {
        JS = jsRuntime;
    }

    public void InitializeVault(string vaultJsonString)
    {

        try
        {
            var settings = new IntJsonConverter();
            _vaultString = vaultJsonString;
            _vaultData = JsonConvert.DeserializeObject<dynamic>(_vaultString, settings);
                
            _dwellers = GetDwellers();
            _rooms = GetRooms();
            _items = GetItems();

            NotifyPropertyChanged();
 
        } 
        catch (Exception ex)
        {
            _vaultString = String.Empty;
            Log("Unable to convert vault string to JSON Object: " + ex.Message);            
        }
    }

    public void CloseVault()
    {
        _vaultString = String.Empty;
        _vaultData = null;
        NotifyPropertyChanged();
    }
    public bool IsVaultEmpty()
    {
        return _vaultString == String.Empty;
    }

    private List<Dweller> GetDwellers()
    {
        var settings = new IntJsonConverter();
        List<Dweller> dwellers = new();
        if (_vaultData == null)
            return new();

        foreach (var dweller in _vaultData.dwellers.dwellers)
        {
            try
            {
                var dwellerObject = JsonConvert.DeserializeObject<Dweller>(dweller.ToString(), settings);
                dwellers.Add(dwellerObject);

            }
            catch (Exception ex)
            {
                Log("Unable to convert dwellers string to JSON Object: " + ex.Message, dweller.ToString());
            }
        }

        return dwellers;
    }

    private List<Room> GetRooms()
    {
        var settings = new IntJsonConverter();
        List<Room> rooms = new();

        foreach (var room in _vaultData?.vault.rooms ?? new List<Room>())
        {
            try
            {
                rooms.Add(JsonConvert.DeserializeObject<Room>(room.ToString(), settings));
            }
            catch (Exception ex)
            {
                Log("Unable to convert rooms string to JSON Object: " + ex.Message);
            }
        }

        return rooms;
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
}
