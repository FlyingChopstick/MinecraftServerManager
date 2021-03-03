using CommonFunctionality;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerManager
{
    public class Backup
    {
        private readonly object _lock = new object();

        public delegate void BackupStartedHandler();
        public delegate void BackupCompleteHandler(string path);
        public delegate void BackupFailedHandler(Exception ex);
        public event BackupStartedHandler Started;
        public event BackupCompleteHandler Complete;
        public event BackupFailedHandler Failed;

        public Backup()
        {
            Config.RobocopyComplete += Config_RobocopyCompleteAsync;
            Config.RobocopyFailed += Config_RobocopyFailedAsync;
        }

        private async void Config_RobocopyCompleteAsync(Operation operation, string destination)
        {
            if (operation == Operation.Backup)
            {
                await LogBackupSuccessAsync(destination);
                Complete?.Invoke(destination);
            }
        }
        private async void Config_RobocopyFailedAsync(Operation operation, Exception ex)
        {
            if (operation == Operation.Backup)
            {
                await LogBackupErrorAsync(ex);
                Failed?.Invoke(ex);
            }
        }


        public Task<string> BackupServerAsync()
        {
            Started?.Invoke();
            return Task.Run(async () =>
            {
                var backupDirExtra = $"{Config.SelectedBackupDir}\\{Path.GetFileName(Config.SelectedServerDir)}";

                var backupDirInfo = new DirectoryInfo(backupDirExtra);

                //if directory does not exist, create it
                if (!backupDirInfo.Exists)
                {
                    Directory.CreateDirectory(backupDirInfo.FullName);
                }
                else
                {
                    //get all directories containing the backup format
                    var activeBackups = backupDirInfo.GetDirectories().Where(d => d.Name.Contains(Config.BackupFormat)).ToList();

                    //if there are existing backups
                    if (activeBackups.Count > 0)
                    {
                        //if the backup max count reached
                        if (activeBackups.Count >= Config.BackupMaxCount)
                        {
                            //find the oldest backup
                            DirectoryInfo oldestBackup = activeBackups[0];
                            for (int i = 1; i < activeBackups.Count; i++)
                            {
                                if (activeBackups[i].CreationTime < oldestBackup.CreationTime)
                                    oldestBackup = activeBackups[i];
                            }

                            //delete the oldest backup
                            activeBackups.Remove(oldestBackup);
                            oldestBackup.Delete(true);
                        }
                    }
                }

                Config.CreateMemoryFile(Config.SelectedServerDir, backupDirExtra);


                string backupName = $"{Config.BackupFormat}{DateTime.Now.ToString(Config.DateTimeFormat)}";
                string backupPath = $"{backupDirExtra}\\{backupName}";
                try
                {
                    //create a new backup directory
                    var newBackupDirInfo = Directory.CreateDirectory(backupPath);


                    //Config.Robocopy(Config.SelectedServerDir, backupPath);
                    await Config.RobocopyAsync(Operation.Backup, Config.SelectedServerDir, backupPath);

                    //RobocopyAsync(Config.SelectedServerDir, backupPath);
                    return backupPath;
                }
                catch (Exception ex)
                {
                    Directory.Delete(backupPath, true);
                    await LogBackupErrorAsync(ex);
                    Failed?.Invoke(ex);
                    return string.Empty;
                }
            });
        }

        private Task LogBackupSuccessAsync(string backupPath)
        {
            return Task.Run(() =>
            {
                lock (_lock)
                {
                    Console.WriteLine($"Backup successful: {backupPath}");
                    File.AppendAllText(Config.LogFilePath,
                        $"[{DateTime.Now}] Backup successful: {backupPath}");
                }
            });
        }
        private Task LogBackupErrorAsync(Exception ex)
        {
            return Task.Run(() =>
            {
                lock (_lock)
                {
                    Console.WriteLine($"Backup failed due to {ex.GetType()}: {ex.Message} \n");
                    Console.WriteLine(ex.StackTrace);
                    File.AppendAllText($"{Config.SelectedBackupDir}\\{Config.LogFilePath}",
                        $"[{DateTime.Now}] Backup failed due to {ex.GetType()}: {ex.Message} \n");
                }
            });
        }
    }
}
