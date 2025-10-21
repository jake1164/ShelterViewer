using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ShelterViewer.Shared.Services.Cryptography;
using ShelterViewer.Shared.Services.VaultServices;

namespace ShelterViewer.Shared.Services.VaultServices;

/// <summary>
/// Browser-friendly implementation that relies on the existing shelter.js helpers.
/// </summary>
public sealed class BrowserVaultFileService : BaseVaultFileService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserVaultFileService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override VaultFilePickerMode PickerMode => VaultFilePickerMode.BrowserInput;

    public override async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
    {
        if (fileInput.Context is null)
        {
            throw new InvalidOperationException("A valid file input element is required for browser vault loading.");
        }

        var content = await _jsRuntime.InvokeAsync<string>("shelter.readFileAsBase64", fileInput);

        // Check if the file appears to be a JSON file (unencrypted)
        if (content.StartsWith("eyJ") || content.StartsWith("ew")) // These are common base64 starts for JSON objects
        {
            // Try to treat it as a direct JSON file
            try
            {
                var jsonString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content));
                if (jsonString.StartsWith("{"))
                {
                    _lastFileWasEncrypted = false;
                    return jsonString;
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error decoding base64/JSON: {ex.Message}"); /* If this fails, continue with decryption attempt */ }
        }

        // Use JavaScript decryption for encrypted files
        try
        {
            var jsonString = await _jsRuntime.InvokeAsync<string>("shelter.decryptString", content);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                _lastFileWasEncrypted = true;
                return jsonString;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting file: {ex.Message}");
        }

        return null;
    }

    public override async Task SaveVaultAsync(string vaultName, string vaultJson)
    {
        if (vaultJson is null)
        {
            throw new ArgumentNullException(nameof(vaultJson));
        }

        var (fileName, encrypt) = GetSaveDefaults(vaultName);

        if (encrypt)
        {
            // For encrypted saves, use the JavaScript encryption
            var encryptedContent = await _jsRuntime.InvokeAsync<string>("shelter.encryptString", vaultJson);
            await _jsRuntime.InvokeVoidAsync("shelter.downloadFile", fileName, encryptedContent, "application/octet-stream");
        }
        else
        {
            // For unencrypted saves, just download as JSON
            await _jsRuntime.InvokeVoidAsync("shelter.downloadFile", fileName, vaultJson, "application/json");
        }
    }

    /// <summary>
    /// Override to use JavaScript implementation for decryption
    /// </summary>
    public override string DecryptToJson(string base64CipherText)
    {
        // Use JS interop synchronously for decryption
        try
        {
            // Since JS interop is async, but base method is sync, block until result is available
            return _jsRuntime.InvokeAsync<string>("shelter.decryptString", base64CipherText).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt using JavaScript interop", ex);
        }
    }

    /// <summary>
    /// Override to use JavaScript implementation for encryption
    /// </summary>
    public override string EncryptFromJson(string jsonPlainText)
    {
        // Use JS interop synchronously for encryption
        try
        {
            return _jsRuntime.InvokeAsync<string>("shelter.encryptString", jsonPlainText).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to encrypt using JavaScript interop", ex);
        }
    }

    /// <summary>
    /// Creates a browser-compatible crypto provider using JavaScript interop
    /// </summary>
    /// <returns>An implementation of ICryptoProvider that works in browser environments</returns>
    protected override ICryptoProvider CreateCryptoProvider()
    {
        return new BrowserCryptoProvider(_jsRuntime);
    }
}
