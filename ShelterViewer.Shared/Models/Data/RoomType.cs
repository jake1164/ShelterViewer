// File: Models/Room.cs

using System.Text.Json.Serialization;

namespace ShelterViewer.Shared.Models.Data;

public class RoomTypeRoot
{
    [JsonPropertyName("rooms")]
    public List<RoomType> Rooms { get; set; } = new();
}

public class RoomType
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("size")]
    public int[] Size { get; set; } = null!;

    [JsonPropertyName("trait")]
    public string? Trait { get; set; }

    [JsonPropertyName("output_type")]
    public string[]? OutputType { get; set; }

    [JsonPropertyName("output")]
    public int[]? Output { get; set; }

    [JsonPropertyName("storage")]
    public int[]? Storage { get; set; }

    [JsonPropertyName("capacity")]
    public int[]? Capacity { get; set; }

    [JsonPropertyName("power_per_min")]
    public double[]? PowerPerMin { get; set; }
}
