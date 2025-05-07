using System.Security.Cryptography;
using System.Text;

public static class SecureHelper
{
    public static string Encrypt(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string encryptedText)
    {
        try
        {
            var bytes = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return "[Decryption Failed]";
        }
    }
}
