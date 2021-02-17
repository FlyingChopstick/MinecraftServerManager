using CommonFunctionality;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBackup
{
    public static class ServerController
    {
        //backup controller knows about
        // server dir 
        // backup dir
        // memory file
        // memory file format

        public delegate void ServerStatusHandler();
        public static event ServerStatusHandler ServerStarted;
        public static event ServerStatusHandler ServerStopped;

        public delegate void BackupStatusHandler();
        public delegate void BackupCompleteHandler(string path);
        public static event BackupStatusHandler BackupStarted;
        public static event BackupCompleteHandler BackupComplete;
        public static event BackupStatusHandler BackupFailed;


        private static Process _server;
        //private static IntPtr _serverHandle;
        public static Process ServerProcess { get => _server; }

        public static void StartServer()
        {
            Directory.SetCurrentDirectory(Config.OriginDirectory);

            Config.CreateMemory(Config.OriginDirectory, Config.BackupDirectory);

            //launch the server
            ServerStarted?.Invoke();
            var server = Process.Start("Start Spigot.bat");
            server.WaitForExit();
        }
        public static Task StartServerAsync()
        {
            ServerStarted?.Invoke();
            return Task.Run(() =>
            {
                Directory.SetCurrentDirectory(Config.OriginDirectory);

                Config.CreateMemory(Config.OriginDirectory, Config.BackupDirectory);

                //launch the server
                _server = new Process();

                _server.StartInfo.WorkingDirectory = Config.OriginDirectory;

                _server.StartInfo.FileName = "cmd.exe";
                _server.StartInfo.Arguments = "/C java -d64 -Xms2G -Xmx4G -XX:+AlwaysPreTouch -XX:+DisableExplicitGC -XX:+UseG1GC -XX:+UnlockExperimentalVMOptions -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=4M -XX:TargetSurvivorRatio=90 -XX:G1NewSizePercent=50 -XX:G1MaxNewSizePercent=80 -XX:InitiatingHeapOccupancyPercent=10 -XX:G1MixedGCLiveThresholdPercent=50 -XX:+AggressiveOpts -jar spigot1.16.5.jar nogui";


                _server.EnableRaisingEvents = true;
                _server.Start();
                _server.Exited += Server_Exited;
            });
        }

        private static void Server_Exited(object sender, EventArgs e)
        {
            ServerStopped?.Invoke();
        }

        //private static void server_Exited(object sender, EventArgs e)
        //{
        //    ServerStopped.Invoke();
        //}

        public static void BackupPrompt(bool shouldAsk = true)
        {
            //backup prompt
            bool shouldBackup = true;
            while (shouldAsk)
            {
                Console.Clear();
                Console.Write("Perform server backup? [y/n] : ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "y":
                        {
                            shouldBackup = true;
                            shouldAsk = false;
                            break;
                        }
                    case "n":
                        {
                            shouldBackup = false;
                            shouldAsk = false;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            if (shouldBackup)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("Starting backup...");

                    string backupPath = Backup();
                    LogBackupSuccess(backupPath);
                }
                catch (Exception ex)
                {
                    LogBackupError(ex);
                }
            }
        }

        private static string Backup()
        {
            var backupDirInfo = new DirectoryInfo(Config.BackupDirectory);

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

            Config.CreateMemory(Config.OriginDirectory, Config.BackupDirectory);


            string backupName = $"{Config.BackupFormat}{DateTime.Now.ToString(Config.DateTimeFormat)}";
            string backupPath = $"{Config.BackupDirectory}\\{backupName}";
            try
            {
                //create a new backup directory
                var newBackupDirInfo = Directory.CreateDirectory(backupPath);


                Config.Robocopy(Config.OriginDirectory, backupPath);


                return backupPath;
            }
            catch (Exception)
            {
                Directory.Delete(backupPath, true);
                throw;
            }
        }
        //public static Task<string> BackupAsync()
        //{
        //    return Task.Run(() =>
        //    {
        //        var backupDirInfo = new DirectoryInfo(Config.BackupDirectory);

        //        //if directory does not exist, create it
        //        if (!backupDirInfo.Exists)
        //        {
        //            Directory.CreateDirectory(backupDirInfo.FullName);
        //        }
        //        else
        //        {
        //            //get all directories containing the backup format
        //            var activeBackups = backupDirInfo.GetDirectories().Where(d => d.Name.Contains(Config.BackupFormat)).ToList();

        //            //if there are existing backups
        //            if (activeBackups.Count > 0)
        //            {
        //                //if the backup max count reached
        //                if (activeBackups.Count >= Config.BackupMaxCount)
        //                {
        //                    //find the oldest backup
        //                    DirectoryInfo oldestBackup = activeBackups[0];
        //                    for (int i = 1; i < activeBackups.Count; i++)
        //                    {
        //                        if (activeBackups[i].CreationTime < oldestBackup.CreationTime)
        //                            oldestBackup = activeBackups[i];
        //                    }

        //                    //delete the oldest backup
        //                    activeBackups.Remove(oldestBackup);
        //                    oldestBackup.Delete(true);
        //                }
        //            }
        //        }

        //        Config.CreateMemory(Config.OriginDirectory, Config.BackupDirectory);


        //        string backupName = $"{Config.BackupFormat}{DateTime.Now.ToString(Config.DateTimeFormat)}";
        //        string backupPath = $"{Config.BackupDirectory}\\{backupName}";
        //        try
        //        {
        //            //create a new backup directory
        //            var newBackupDirInfo = Directory.CreateDirectory(backupPath);


        //            Config.Robocopy(Config.OriginDirectory, backupPath);


        //            return backupPath;
        //        }
        //        catch (Exception)
        //        {
        //            Directory.Delete(backupPath, true);
        //            throw;
        //        }
        //    });
        //}

        public static Task<string> BackupServerAsync()
        {
            BackupStarted?.Invoke();
            return Task.Run(async () =>
            {
                var backupDirInfo = new DirectoryInfo(Config.BackupDirectory);

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

                Config.CreateMemory(Config.OriginDirectory, Config.BackupDirectory);


                string backupName = $"{Config.BackupFormat}{DateTime.Now.ToString(Config.DateTimeFormat)}";
                string backupPath = $"{Config.BackupDirectory}\\{backupName}";
                try
                {
                    //create a new backup directory
                    var newBackupDirInfo = Directory.CreateDirectory(backupPath);


                    //Config.Robocopy(Config.OriginDirectory, backupPath);
                    string res = await RobocopyAsync(Config.OriginDirectory, backupPath);

                    //RobocopyAsync(Config.OriginDirectory, backupPath);
                    BackupComplete?.Invoke(res);
                    return backupPath;
                }
                catch (Exception)
                {
                    Directory.Delete(backupPath, true);
                    BackupFailed?.Invoke();
                    return string.Empty;
                }
            });
        }


        public static Task<string> RobocopyAsync(string source, string destination)
        {
            return Task.Run(() =>
            {
                try
                {
                    //robocopy the source directory into new backup directory
                    Process robocopy = new Process();
                    robocopy.StartInfo.FileName = Config.RobocopyExe;
                    robocopy.StartInfo.Arguments = $"\"{source}\" " + $"\"{destination}\" " + Config.RobocopyArgs;
                    robocopy.StartInfo.CreateNoWindow = true;
                    robocopy.StartInfo.UseShellExecute = false;

                    robocopy.Start();
                    robocopy.Exited += Robocopy_Exited;
                    return destination;
                    //robocopy.WaitForExit();
                }
                catch (Exception)
                {
                    Directory.Delete(destination, true);

                    throw;
                }
            });
        }

        private static void Robocopy_Exited(object sender, EventArgs e)
        {
        }

        private static void LogBackupSuccess(string backupPath)
        {
            Console.WriteLine($"Backup successful: {backupPath}");
            File.AppendAllText(Config.LogFilePath,
                $"[{DateTime.Now}] Backup successful: {backupPath}");
        }
        private static void LogBackupError(Exception ex)
        {
            Console.WriteLine($"Backup failed due to {ex.GetType()}: {ex.Message} \n");
            Console.WriteLine(ex.StackTrace);
            File.AppendAllText($"{Config.BackupDirectory}\\{Config.LogFilePath}",
                $"[{DateTime.Now}] Backup failed due to {ex.GetType()}: {ex.Message} \n");
        }
    }
}
