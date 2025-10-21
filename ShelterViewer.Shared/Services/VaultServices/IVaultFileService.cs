using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ShelterViewer.Shared.Services.VaultServices;

/// <summary>
/// Abstraction for loading and saving vault files across platforms.
/// </summary>
public interface IVaultFileService
{
    VaultFilePickerMode PickerMode { get; }

    Task<string?> OpenVaultAsync(ElementReference fileInput = default);

    Task SaveVaultAsync(string vaultName, string vaultJson);
}
