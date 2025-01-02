using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ShelterViewer.Shared.Models;

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

    // Added fields for display
    public int AssignedRoom { get; set; }

    // Added fields for displaying parents and children
    public string Name { get { return $"{firstName} {lastName}"; } }
    public string Gender { get { return (_gender == 1) ? "Female" : "Male"; } }
    public Dweller? Mother { get; set; }
    public Dweller? Father { get; set; }
    public List<Dweller> Children { get; set; } = new();
    public Stats.MaxStat[] MaxStats { get { return stats.MaxStats; } }
}

public class Stats
{
    public enum SpecialStats
    {
        Strength = 1,
        Perception = 2,
        Endurance = 3,
        Charisma = 4,
        Intelligence = 5,
        Agility = 6,
        Luck = 7
    }
    public record MaxStat(SpecialStats StatName, int Value);
    public MaxStat[] MaxStats
    {
        get
        {
            // Find the maximum Value including the Mod across all stats
            int maxModValue = SPECIAL.Max(s => s.ValueWithMod);

            // Return all stats that have this maximum ModValue
            return SPECIAL
                .Where(s => s.ValueWithMod == maxModValue)
                .Select(s => new MaxStat(s.Name, s.ValueWithMod))
                .ToArray();
        }
    }
    private Stat[] _stats { get; set; } = null!;
    [JsonPropertyName("stats")]
    [JsonProperty("stats")]
    public Stat[] SPECIAL 
    { 
        get {  return _stats; }
        set 
        { 
            // I cant make this a 0 based array because that will break eventual export.
            for (int i = 1; i < value.Length; i++)
            {
                value[i].Name = (SpecialStats)i;
            }
            _stats = value;
        }
    }
}

public class Stat
{ 
    public Stats.SpecialStats Name { get; set; }
    public int value { get; set; }
    public int mod { get; set; }
    public float exp { get; set; }
    public int ValueWithMod { get { return value + mod; } }
}