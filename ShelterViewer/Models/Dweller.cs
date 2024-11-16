using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ShelterViewer.Models;

public class Dweller
{
    public int serializeId { get; set; }
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string firstName { get; set; } = null!;
    public string lastName { get; set; } = null!;
    public Happiness happiness { get; set; } = null!;
    public Health health { get; set; } = null!;
    public Experience experience { get; set; } = null!;
    public Relations relations { get; set; } = null!;
    [JsonPropertyName("gender")]
    [JsonProperty("gender")]
    public int _gender { get; set; }
    public Stats stats { get; set; } = null!;
    public bool pregnant { get; set; }
    public bool babyReady { get; set; }
    public bool assigned { get; set; }
    public bool sawIncident { get; set; }
    public bool WillGoToWasteland { get; set; }
    public bool WillBeEvicted { get; set; }
    public bool IsEvictedWaitingForFollowers { get; set; }
    public long skinColor { get; set; }
    public long hairColor { get; set; }
    public long outfitColor { get; set; }
    public int pendingExperienceReward { get; set; }
    public string hair { get; set; } = null!;
    public EquippedOutfit equipedOutfit { get; set; } = null!;
    public EquippedWeapon equipedWeapon { get; set; } = null!;
    public int savedRoom { get; set; }
    public float lastChildBorn { get; set; }
    public string rarity { get; set; } = null!;
    public int deathTime { get; set; }
    public string faceMask { get; set; } = null!;
    public EquippedPet? equippedPet { get; set; }
    public string uniqueData { get; set; } = null!;
    public int daysOnWasteland { get; set; }
    public int hoursOnWasteland { get; set; }

    // Added fields for displaying parents and children
    public string Name { get { return $"{firstName} {lastName}"; } }
    public string Gender { get { return (_gender == 1) ? "Female" : "Male"; } }
    public Dweller? Mother { get; set; }
    public Dweller? Father { get; set; }
    public List<Dweller> Children { get; set; } = new();
}
