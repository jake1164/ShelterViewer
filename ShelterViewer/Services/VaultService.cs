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

namespace ShelterViewer.Services;

public class VaultService
{
    public event Action? OnVaultChanged = null;

    private IJSRuntime JS;
    private string _vaultString = String.Empty;

    private dynamic? _vaultData = null;

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

    public List<Dweller> GetDwellers()
    {
        var settings = new IntJsonConverter();
        List<Dweller> dwellers = new();
        if (_vaultData == null)
            return new();

        foreach (var dweller in _vaultData.dwellers.dwellers)
        {
            try
            {
                Log(dweller);
                dwellers.Add(JsonConvert.DeserializeObject<Dweller>(dweller.ToString(), settings));

            }
            catch (Exception ex)
            {
                Log("Unable to convert dwellers string to JSON Object: " + ex.Message);
            }
        }

        return dwellers;
    }

    public List<Room> GetRooms()
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
