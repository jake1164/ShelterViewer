using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using ShelterViewer.Shared.Services.VaultServices;
using ShelterViewer.Utility;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ShelterViewer.Services;

public class WindowsVaultFileService : IVaultFileService
{
    private bool _lastFileWasEncrypted;
    private string? _lastLoadedFileBaseName;

    public VaultFilePickerMode PickerMode => VaultFilePickerMode.NativeDialog;

    public async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        picker.FileTypeFilter.Add(".sav");
        picker.FileTypeFilter.Add(".json");

        InitializeWithWindow.Initialize(picker, GetActiveWindowHandle());

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return null;
        }

        using var stream = await file.OpenStreamForReadAsync();
        using var reader = new StreamReader(stream);
        var rawContent = await reader.ReadToEndAsync();

        var isEncrypted = IsSav(Path.GetExtension(file.Name));
        _lastFileWasEncrypted = isEncrypted;
        _lastLoadedFileBaseName = Path.GetFileNameWithoutExtension(file.Name);

        return isEncrypted ? ShelterVaultCrypto.DecryptToJson(rawContent) : rawContent;
    }

    public async Task SaveVaultAsync(string vaultName, string vaultJson)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        InitializeWithWindow.Initialize(picker, GetActiveWindowHandle());

        // Determine defaults based on prior load state.
        var (suggestedName, encryptPayload) = GetSaveDefaults(vaultName);
        var baseName = Path.GetFileNameWithoutExtension(suggestedName);

        if (_lastFileWasEncrypted)
        {
            picker.FileTypeChoices.Add("Fallout Shelter Save (*.sav)", new List<string> { ".sav" });
            picker.FileTypeChoices.Add("JSON file (*.json)", new List<string> { ".json" });
        }
        else
        {
            picker.FileTypeChoices.Add("JSON file (*.json)", new List<string> { ".json" });
            picker.FileTypeChoices.Add("Fallout Shelter Save (*.sav)", new List<string> { ".sav" });
        }

        picker.SuggestedFileName = baseName;

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        var extension = Path.GetExtension(file.Name);
        var payload = BuildPayload(vaultJson, encryptPayload || IsSav(extension));

        await FileIO.WriteTextAsync(file, payload);
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

    private static IntPtr GetActiveWindowHandle()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is MauiWinUIWindow mauiWindow)
        {
            return WindowNative.GetWindowHandle(mauiWindow);
        }

        throw new InvalidOperationException("Unable to locate an active window handle for the current application.");
    }

    private static bool IsSav(string? extension) =>
        string.Equals(extension, ".sav", StringComparison.OrdinalIgnoreCase);

    private static string BuildPayload(string vaultJson, bool encrypt) =>
        encrypt ? ShelterVaultCrypto.EncryptFromJson(vaultJson) : vaultJson;

    private static string SanitizeFileName(string? candidate, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate!;
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
