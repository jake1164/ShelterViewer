namespace ShelterViewer.Shared.Models;
public interface IItem
{
    public string? id { get; set; }
    public string? type { get; set; }
    public bool hasBeenAssigned { get; set; }
    public bool hasRandonWeaponBeenAssigned { get; set; }
}