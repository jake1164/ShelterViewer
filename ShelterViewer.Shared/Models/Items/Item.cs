namespace ShelterViewer.Shared.Models;

public class EquippedOutfit : Outfit, IItem
{ }

public class EquippedWeapon : Weapon, IItem
{ }

public class EquippedPet : Pet, IItem
{ }

public class Item : IItem
{
    public string? id { get; set; }
    public string? type { get; set; }
    public bool hasBeenAssigned { get; set; }
    public bool hasRandonWeaponBeenAssigned { get; set; }
    public ExtraItemdata? extraData { get; set; }
}
public class Weapon : IItem
{
    public string? id { get; set; }
    public string? type { get; set; }
    public bool hasBeenAssigned { get; set; }
    public bool hasRandonWeaponBeenAssigned { get; set; }
}

public class Outfit : IItem
{
    public string? id { get; set; }
    public string? type { get; set; }
    public bool hasBeenAssigned { get; set; }
    public bool hasRandonWeaponBeenAssigned { get; set; }
}

public class Pet : IItem
{
    public string? id { get; set; }
    public string? type { get; set; }
    public bool hasBeenAssigned { get; set; }
    public bool hasRandonWeaponBeenAssigned { get; set; }
    public ExtraItemdata? extraData { get; set; }
}

public class ExtraItemdata
{
    public string? uniqueName { get; set; }
    public string? bonus { get; set; }
    public float bonusValue { get; set; }
}