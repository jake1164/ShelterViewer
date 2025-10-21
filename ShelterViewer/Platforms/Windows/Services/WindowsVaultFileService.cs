using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using ShelterViewer.Services.Cryptography;
using ShelterViewer.Shared.Services.VaultServices;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ShelterViewer.Services;

public class WindowsVaultFileService : BaseVaultFileService
{
    public override VaultFilePickerMode PickerMode => VaultFilePickerMode.NativeDialog;

    public override async Task<string?> OpenVaultAsync(ElementReference fileInput = default)
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

        return isEncrypted ? DecryptToJson(rawContent) : rawContent;
    }

    public override async Task SaveVaultAsync(string vaultName, string vaultJson)
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

    private static IntPtr GetActiveWindowHandle()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is MauiWinUIWindow mauiWindow)
        {
            return WindowNative.GetWindowHandle(mauiWindow);
        }

        throw new InvalidOperationException("Unable to locate an active window handle for the current application.");
    }

    /// <summary>
    /// Creates a Windows-compatible AesCryptoProvider for native encryption/decryption
    /// </summary>
    /// <returns>An implementation of ICryptoProvider using System.Security.Cryptography.Aes</returns>
    protected override ICryptoProvider CreateCryptoProvider()
    {
        return new AesCryptoProvider(EncryptionKey, InitializationVector);
    }
}
