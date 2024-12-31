using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterViewer.Shared.Models;

namespace ShelterViewer.Shared.Extensions;

public static class RoomExtension
{
    public static Stats.SpecialStats? ToSpecialStat(this string trait)
    {
        return trait.ToLower() switch
        {
            "strength" => Stats.SpecialStats.Strength,
            "perception" => Stats.SpecialStats.Perception,
            "endurance" => Stats.SpecialStats.Endurance,
            "charisma" => Stats.SpecialStats.Charisma,
            "intelligence" => Stats.SpecialStats.Intelligence,
            "agility" => Stats.SpecialStats.Agility,
            "luck" => Stats.SpecialStats.Luck,
            _ => null
        };
    }
}
