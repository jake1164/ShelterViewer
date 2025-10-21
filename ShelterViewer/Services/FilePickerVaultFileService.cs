using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using ShelterViewer.Shared.Services.VaultServices;
using ShelterViewer.Utility;

namespace ShelterViewer.Services;

/// <summary>
/// Provides a native vault picker/save experience using MAUI cross-platform file picker and share APIs.
/// Intended for Android, iOS, and Mac Catalyst targets.
/// </summary>
public class FilePickerVaultFileService : IVaultFileService
{
    private static readonly IReadOnlyDictionary<DevicePlatform, IEnumerable<string>> DefaultFileTypes =
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] = new[] { ".sav", ".json" },
            [DevicePlatform.iOS] = new[] { ".sav", ".json" },
            [DevicePlatform.macOS] = new[] { ".sav", ".json" },
            [DevicePlatform.WinUI] = new[] { ".sav", ".json" }
        };

    private bool _lastFileWasEncrypted;
    private string? _lastLoadedFileBaseName;

    public VaultFilePickerMode PickerMode => VaultFilePickerMode.NativeDialog;

    public async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
    {
        var pickOptions = new PickOptions
        {
            PickerTitle = "Select Fallout Shelter save",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>(DefaultFileTypes))
        };

        var fileResult = await FilePicker.Default.PickAsync(pickOptions);
        if (fileResult is null)
        {
            return null;
        }

        using var stream = await fileResult.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var rawContent = await reader.ReadToEndAsync();

        var isEncrypted = IsSav(Path.GetExtension(fileResult.FileName));
        _lastFileWasEncrypted = isEncrypted;
        _lastLoadedFileBaseName = Path.GetFileNameWithoutExtension(fileResult.FileName);

        return isEncrypted ? ShelterVaultCrypto.DecryptToJson(rawContent) : rawContent;
    }

    public async Task SaveVaultAsync(string vaultName, string vaultJson)
    {
        if (string.IsNullOrWhiteSpace(vaultJson))
        {
            throw new ArgumentException("Vault content cannot be empty.", nameof(vaultJson));
        }

        var (suggestedName, encryptPayload) = GetSaveDefaults(vaultName);
        var payload = encryptPayload ? ShelterVaultCrypto.EncryptFromJson(vaultJson) : vaultJson;

        var tempDirectory = FileSystem.Current.CacheDirectory;
        var targetPath = Path.Combine(tempDirectory, suggestedName);
        await File.WriteAllTextAsync(targetPath, payload);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share vault file",
            File = new ShareFile(targetPath)
        });
    }

    private (string SuggestedName, bool EncryptPayload) GetSaveDefaults(string vaultName)
    {
        if (_lastFileWasEncrypted)
        {
            var sourceName = SanitizeFileName(_lastLoadedFileBaseName, "Vault");
            return ($"{sourceName}.sav", true);
        }

        var safeVault = SanitizeFileName(vaultName, "Vault");
        return ($"Vault{safeVault}.json", false);
    }

    private static bool IsSav(string? extension) =>
        string.Equals(extension, ".sav", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeFileName(string? candidate, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate!;
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
