using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ShelterViewer.Shared.Services.VaultServices;

/// <summary>
/// Browser-friendly implementation that relies on the existing shelter.js helpers.
/// </summary>
public sealed class BrowserVaultFileService : IVaultFileService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserVaultFileService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public VaultFilePickerMode PickerMode => VaultFilePickerMode.BrowserInput;

    public async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
    {
        if (fileInput.Context is null)
        {
            throw new InvalidOperationException("A valid file input element is required for browser vault loading.");
        }

        var content = await _jsRuntime.InvokeAsync<string>("shelter.readFileAsBase64", fileInput);
        var jsonString = await _jsRuntime.InvokeAsync<string>("shelter.decryptString", content);
        return string.IsNullOrWhiteSpace(jsonString) ? null : jsonString;
    }

    public async Task SaveVaultAsync(string vaultName, string vaultJson)
    {
        if (vaultJson is null)
        {
            throw new ArgumentNullException(nameof(vaultJson));
        }

        var fileName = $"Vault{vaultName}.json";
        await _jsRuntime.InvokeVoidAsync("shelter.downloadFile", fileName, vaultJson, "application/json");
    }
}
