using System;
using System.Text;
using Microsoft.JSInterop;
using ShelterViewer.Shared.Services.VaultServices;

namespace ShelterViewer.Shared.Services.Cryptography;

/// <summary>
/// Browser-specific implementation of ICryptoProvider that uses JavaScript interop
/// for encryption and decryption operations, since System.Security.Cryptography.Aes
/// is not available in browser environments.
/// </summary>
public class BrowserCryptoProvider : ICryptoProvider
{
    private readonly IJSRuntime _jsRuntime;
    private bool _disposed;

    public BrowserCryptoProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public string DecryptToString(byte[] cipherBytes)
  {
        // Convert bytes to Base64 for JavaScript interop
        string base64 = Convert.ToBase64String(cipherBytes);
        
        // Use JS interop synchronously for decryption
        try
   {
         // Since JS interop is async, but this method is sync, block until result is available
        return _jsRuntime.InvokeAsync<string>("shelter.decryptString", base64).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
         throw new InvalidOperationException("Failed to decrypt using JavaScript interop", ex);
     }
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        // Convert plain bytes to string for JS encryption
        string plainText = Encoding.UTF8.GetString(plainBytes);
        
     // Use JS interop synchronously for encryption
   try
        {
       string base64Result = _jsRuntime.InvokeAsync<string>("shelter.encryptString", plainText).GetAwaiter().GetResult();
          return Convert.FromBase64String(base64Result);
        }
        catch (Exception ex)
      {
  throw new InvalidOperationException("Failed to encrypt using JavaScript interop", ex);
        }
 }

    public void Dispose()
    {
        if (!_disposed)
        {
// Nothing to dispose here as JSRuntime is not owned by this instance
            _disposed = true;
  }
    }
}