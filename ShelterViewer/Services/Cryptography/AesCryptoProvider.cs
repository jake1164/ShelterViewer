using System.Security.Cryptography;
using System.Text;
using ShelterViewer.Shared.Services.VaultServices;

namespace ShelterViewer.Services.Cryptography;

/// <summary>
/// Native implementation of ICryptoProvider using System.Security.Cryptography.Aes
/// This implementation is used for non-browser platforms (Windows, Android, iOS, MacCatalyst).
/// </summary>
public class AesCryptoProvider : ICryptoProvider
{
    private readonly Aes _aes;

    public AesCryptoProvider(byte[] key, byte[] iv)
{
        _aes = Aes.Create();
        _aes.Mode = CipherMode.CBC;
 _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = key;
        _aes.IV = iv;
    }

    public string DecryptToString(byte[] cipherBytes)
    {
        using var decryptor = _aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        using var encryptor = _aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    public void Dispose()
    {
        _aes.Dispose();
    }
}