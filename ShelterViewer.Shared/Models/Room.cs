using Newtonsoft.Json;

namespace ShelterViewer.Shared.Models;

public class Room
{
    public bool emergencyDone { get; set; }
    public string type { get; set; } = null!;
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
    public int[] dwellers { get; set; } = null!;
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
    public int[]? Size { get; set; }
    public string[]? OutputType { get; set; }
    public int[]? Output { get; set; }
    public int[]? Storage { get; set; }
    public int[]? Capacity { get; set; }
    public double[]? PowerPerMin { get; set; }
}