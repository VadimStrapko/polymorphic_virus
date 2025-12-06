using System;
using System.Linq;

namespace CP.Services
{
    public class ActivityLoggerService
    {
        public void LogEncryptionActivity(int victimId, int filesCount, string encryptionKey)
        {
            try
            {
                using (var context = new virus_dbEntities2())
                {
                    var activity = new activity_log
                    {
                        victim_id = victimId, // int, а не string!
                        action_type = "encryption",
                        action_details = $"Зашифровано {filesCount} файлов. Ключ: {encryptionKey}",
                        action_time = DateTime.Now
                    };

                    context.activity_log.Add(activity);
                    context.SaveChanges();
                }
            }
            catch { }
        }

        public void LogDecryptionActivity(int victimId, int filesCount)
        {
            try
            {
                using (var context = new virus_dbEntities2())
                {
                    var activity = new activity_log
                    {
                        victim_id = victimId,
                        action_type = "decryption",
                        action_details = $"Расшифровано {filesCount} файлов",
                        action_time = DateTime.Now
                    };

                    context.activity_log.Add(activity);
                    context.SaveChanges();
                }
            }
            catch { }
        }
    }
}