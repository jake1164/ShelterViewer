using System;
using System.Security.Cryptography;
using System.Text;

namespace ShelterViewer.Utility;

/// <summary>
/// Native AES helpers aligned with the original shelter.js implementation.
/// </summary>
internal static class ShelterVaultCrypto
{
    private static readonly byte[] EncryptionKey =
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

    private static readonly byte[] InitializationVector = Convert.FromHexString("7475383967656A693334307438397532");

    public static string DecryptToJson(string base64CipherText)
    {
        if (string.IsNullOrWhiteSpace(base64CipherText))
        {
            throw new ArgumentException("Cipher text must not be empty.", nameof(base64CipherText));
        }

        var cipherBytes = Convert.FromBase64String(base64CipherText.Trim());
        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public static string EncryptFromJson(string jsonPlainText)
    {
        if (jsonPlainText is null)
        {
            throw new ArgumentNullException(nameof(jsonPlainText));
        }

        var plainBytes = Encoding.UTF8.GetBytes(jsonPlainText);
        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = EncryptionKey;
        aes.IV = InitializationVector;
        return aes;
    }
}
