using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ShelterViewer.Shared.Models;

public class Room
{
    public bool emergencyDone { get; set; }
    public string type { get; set; } = null!;
    [JsonPropertyName("class")]
    [JsonProperty("class")]
    public string _class { get; set; } = null!;
    public int mergeLevel { get; set; }
    public int row { get; set; }
    public int col { get; set; }
    public bool power { get; set; }
    public Roomhealth roomHealth { get; set; } = null!;
    public int[]? mrHandyList { get; set; }
    public int rushTask { get; set; }
    public int level { get; set; }
    [JsonPropertyName("dwellers")]
    [JsonProperty("dwellers")]
    public int[] DwellerIds { get; set; } = null!;
    public int[]? deadDwellers { get; set; }
    public string? currentStateName { get; set; } 
    public Currentstate? currentState { get; set; }
    public int deserializeID { get; set; }
    public string? assignedDecoration { get; set; }
    public bool roomVisibility { get; set; }
    public bool roomOutline { get; set; }
    public bool broken { get; set; }
    public bool withHole { get; set; }
    public object[] children { get; set; } = null!;
    public object[] partners { get; set; } = null!;
    public Storage3 storage { get; set; } = null!;
    public float numberOfProductionCycle { get; set; }
    public bool ExperienceRewardIsDirty { get; set; }
    public string[]? IngredientItemIds { get; set; }
    public string? CraftingItemId { get; set; }
    public float CompletedTime { get; set; }
    public Slot[] slots { get; set; } = null!;
    public int[]? dwellerWithPendingCompleteTraining { get; set; }
    public bool newDwellerReady { get; set; }
    public bool onlyIncreaseHappiness { get; set; }

    // Added for ShelterViewer.Web.Client
    public string? Name { get; set; }
    public int? Level { get; set; } // Do we need two???
    public string? Trait { get; set; }
    public int Size { get; set; } = 1;
    public string[]? OutputType { get; set; }
    public int? Output { get; set; }
    public int? StorageCapacity { get; set; }
    public int? DwellerCapacity { get; set; }
    public double? PowerPerMin { get; set; }
    public Dweller[]? Dwellers { get; set; } // Populated in VaultService.ProcessRooms()

    // Calculated Values
    public int? MaxRoomSpeed
    {
        get
        {
            if (String.IsNullOrEmpty(Trait) || type == "LivingQuarters" || type == "Storage") return null;
            return level * 2 * 10;
        }
    }

    public int? MaxRoomSpeedWithMod
    {
        get
        {
            if (String.IsNullOrEmpty(Trait)) return null;
            return level * 2 * 17;
        }
    }

    public int? CurrentRoomSpeed
    {
        get
        {
            if (String.IsNullOrEmpty(Trait) || Dwellers == null) return null;
            int speed = 0;
            // Dwellers special attribute matching trait 
            foreach (var dweller in Dwellers)
            {
                var s = (int)Enum.Parse<Stats.SpecialStats>(Trait);
                speed += dweller.stats.SPECIAL[s].value;
            }
            return speed;
        }
    }

    public int? CurrentRoomSpeedWithMod
    {
        get
        {
            if (String.IsNullOrEmpty(Trait) || Dwellers == null) return null;
            int speed = 0;
            // Dwellers special attribute matching trait 
            foreach (var dweller in Dwellers)
            {
                speed += dweller.stats.SPECIAL[(int)Enum.Parse<Stats.SpecialStats>(Trait)].value;
                speed += dweller.stats.SPECIAL[(int)Enum.Parse<Stats.SpecialStats>(Trait)].mod;
            }
            return speed;
        }
    }
}