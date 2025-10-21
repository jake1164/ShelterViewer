using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ShelterViewer.Shared.Services.VaultServices;

/// <summary>
/// Provides a native vault picker/save experience using MAUI cross-platform file picker and share APIs.
/// Intended for Android, iOS, and Mac Catalyst targets.
/// </summary>
public class FilePickerVaultFileService : BaseVaultFileService
{
    private static readonly IReadOnlyDictionary<DevicePlatform, IEnumerable<string>> DefaultFileTypes =
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] = new[] { ".sav", ".json" },
            [DevicePlatform.iOS] = new[] { ".sav", ".json" },
            [DevicePlatform.macOS] = new[] { ".sav", ".json" },
            [DevicePlatform.WinUI] = new[] { ".sav", ".json" }
        };

    public override VaultFilePickerMode PickerMode => VaultFilePickerMode.NativeDialog;

    public override async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
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

        return isEncrypted ? DecryptToJson(rawContent) : rawContent;
    }

    public override async Task SaveVaultAsync(string vaultName, string vaultJson)
    {
        if (string.IsNullOrWhiteSpace(vaultJson))
        {
            throw new ArgumentException("Vault content cannot be empty.", nameof(vaultJson));
        }

        var (suggestedName, encryptPayload) = GetSaveDefaults(vaultName);
        var payload = BuildPayload(vaultJson, encryptPayload);

        var tempDirectory = FileSystem.Current.CacheDirectory;
        var targetPath = Path.Combine(tempDirectory, suggestedName);
        await File.WriteAllTextAsync(targetPath, payload);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share vault file",
            File = new ShareFile(targetPath)
        });
    }
}
