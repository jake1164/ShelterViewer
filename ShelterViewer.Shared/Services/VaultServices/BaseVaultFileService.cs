using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ShelterViewer.Shared.Services.VaultServices
{
    /// <summary>
    /// Base implementation for vault file services, providing common functionality
    /// across different platforms.
    /// </summary>
    public abstract class BaseVaultFileService : IVaultFileService
    {
        // Key and IV are kept protected so derived classes can access them if needed
        protected static readonly byte[] EncryptionKey =
        [
            0xA7, 0xCA, 0x9F, 0x33,
            0x66, 0xD8, 0x92, 0xC2,
        0xF0, 0xBE, 0xF4, 0x17,
  0x34, 0x1C, 0xA9, 0x71,
 0xB6, 0x9A, 0xE9, 0xF7,
            0xBA, 0xCC, 0xCF, 0xFC,
      0xF4, 0x3C, 0x62, 0xD1,
            0xD7, 0xD0, 0x21, 0xF9
        ];

        protected static readonly byte[] InitializationVector = Convert.FromHexString("7475383967656A693334307438397532");

        // Common fields for tracking file state
        protected bool _lastFileWasEncrypted;
        protected string? _lastLoadedFileBaseName;

        /// <summary>
        /// Gets the picker mode for the current implementation.
        /// </summary>
        public abstract VaultFilePickerMode PickerMode { get; }

        /// <summary>
        /// Opens a vault file using platform-specific mechanisms.
        /// </summary>
        /// <param name="fileInput">Optional element reference for browser implementations.</param>
        /// <returns>The JSON content of the vault file, or null if operation was cancelled.</returns>
        public abstract Task<string?> OpenVaultAsync(ElementReference fileInput = default);

        /// <summary>
        /// Saves a vault file using platform-specific mechanisms.
        /// </summary>
        /// <param name="vaultName">Name of the vault to save.</param>
        /// <param name="vaultJson">JSON content to save.</param>
        /// <returns>A task representing the save operation.</returns>
        public abstract Task SaveVaultAsync(string vaultName, string vaultJson);

        /// <summary>
        /// Determines save defaults based on the last file loaded or created.
        /// </summary>
        /// <param name="vaultName">Name of the vault from the vault data.</param>
        /// <returns>A tuple with the suggested filename and whether to encrypt the payload.</returns>
        protected (string SuggestedName, bool EncryptPayload) GetSaveDefaults(string vaultName)
        {
            if (_lastFileWasEncrypted)
            {
                var sourceName = SanitizeFileName(_lastLoadedFileBaseName, "Vault");
                return ($"{sourceName}.sav", true);
            }

            var safeVault = SanitizeFileName(vaultName, "Vault");
            return ($"Vault{safeVault}.json", false);
        }

        /// <summary>
        /// Determines if a file extension indicates a Fallout Shelter save file.
        /// </summary>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>True if the extension indicates an encrypted save file.</returns>
        protected static bool IsSav(string? extension) =>
            string.Equals(extension, ".sav", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Builds the final payload for saving, encrypting if necessary.
        /// </summary>
        /// <param name="vaultJson">The JSON content to potentially encrypt.</param>
        /// <param name="encrypt">Whether to encrypt the content.</param>
        /// <returns>The final string payload ready for saving.</returns>
        protected virtual string BuildPayload(string vaultJson, bool encrypt) =>
    encrypt ? EncryptFromJson(vaultJson) : vaultJson;

        /// <summary>
        /// Sanitizes a filename by replacing invalid characters.
        /// </summary>
        /// <param name="candidate">The candidate filename.</param>
        /// <param name="fallback">Fallback name if candidate is invalid.</param>
        /// <returns>A sanitized filename.</returns>
        protected static string SanitizeFileName(string? candidate, string fallback)
        {
            var value = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate!;
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
        }

        /// <summary>
        /// Decrypts a base64-encoded ciphertext to a JSON string.
        /// </summary>
        /// <param name="base64CipherText">The base64-encoded ciphertext.</param>
        /// <returns>The decrypted JSON string.</returns>
        public virtual string DecryptToJson(string base64CipherText)
        {
            if (string.IsNullOrWhiteSpace(base64CipherText))
            {
                throw new ArgumentException("Cipher text must not be empty.", nameof(base64CipherText));
            }

            var cipherBytes = Convert.FromBase64String(base64CipherText.Trim());
            return DecryptBytesToString(cipherBytes);
        }

        /// <summary>
        /// Encrypts a JSON string to a base64-encoded ciphertext.
        /// </summary>
        /// <param name="jsonPlainText">The JSON string to encrypt.</param>
        /// <returns>The base64-encoded ciphertext.</returns>
        public virtual string EncryptFromJson(string jsonPlainText)
        {
            if (jsonPlainText is null)
            {
                throw new ArgumentNullException(nameof(jsonPlainText));
            }

            var plainBytes = Encoding.UTF8.GetBytes(jsonPlainText);
            var cipherBytes = EncryptStringToBytes(plainBytes);
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// Decrypts cipher bytes to a string using the platform-specific implementation.
        /// </summary>
        /// <param name="cipherBytes">The encrypted bytes.</param>
        /// <returns>The decrypted string.</returns>
        protected virtual string DecryptBytesToString(byte[] cipherBytes)
        {
            using var cryptoProvider = CreateCryptoProvider();
            return cryptoProvider.DecryptToString(cipherBytes);
        }

        /// <summary>
        /// Encrypts plain bytes to cipher bytes using the platform-specific implementation.
        /// </summary>
        /// <param name="plainBytes">The plain text bytes.</param>
        /// <returns>The encrypted bytes.</returns>
        protected virtual byte[] EncryptStringToBytes(byte[] plainBytes)
        {
            using var cryptoProvider = CreateCryptoProvider();
            return cryptoProvider.EncryptBytes(plainBytes);
        }

        /// <summary>
        /// Creates a platform-specific cryptography provider with the correct settings for Fallout Shelter saves.
        /// Each platform must implement this method to provide appropriate encryption/decryption capabilities.
        /// </summary>
        /// <returns>A platform-specific crypto provider implementing ICryptoProvider.</returns>
        protected abstract ICryptoProvider CreateCryptoProvider();
    }

    /// <summary>
    /// Interface for platform-specific cryptography implementations to ensure
    /// consistent encryption/decryption operations across platforms.
    /// </summary>
    public interface ICryptoProvider : IDisposable
    {
        /// <summary>
        /// Decrypts the provided ciphertext to a string using platform-specific mechanisms.
        /// </summary>
        /// <param name="cipherBytes">The encrypted bytes to decrypt.</param>
        /// <returns>The decrypted string.</returns>
        string DecryptToString(byte[] cipherBytes);

        /// <summary>
        /// Encrypts the provided plaintext bytes using platform-specific mechanisms.
        /// </summary>
        /// <param name="plainBytes">The bytes to encrypt.</param>
        /// <returns>The encrypted bytes.</returns>
        byte[] EncryptBytes(byte[] plainBytes);
    }
}