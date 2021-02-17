
using CommonFunctionality;
using System;
using System.IO;
using System.Linq;

namespace ServerRestore
{
    //restore controller knows about
    // backup dir
    // memory file
    // memory file format

    public static class RestoreController
    {
        private static string backupDirectory;
        private static string originDirectory;

        public static void Startup()
        {
            backupDirectory = Config.BackupDirectory;

            var memoryFilePath = $"{backupDirectory}\\{Config.MemoryFile}";
            if (File.Exists(memoryFilePath))
            {
                var originDescriptor = File.ReadAllLines(memoryFilePath).Where(l => l.Contains(Config.MemoryDescription)).FirstOrDefault();
                originDirectory = originDescriptor.Substring(Config.MemoryDescription.Length);
                if (originDirectory is null)
                {
                    Console.WriteLine("Could not determine origin directory from the file.");
                    OriginPrompt();
                }
            }
            else
            {
                Console.WriteLine("Could not find the origin memory file.");
                OriginPrompt();
            }
        }
        private static void OriginPrompt()
        {
            int tries = 3;
            string attempt;

            while (tries > 0)
            {
                Console.Write("Please enter the path to the origin directory: ");
                attempt = Console.ReadLine();
                if (Directory.Exists(attempt))
                {
                    Console.WriteLine("New origin selected.");
                    originDirectory = attempt;

                    //restore memory about the origin
                    Config.CreateMemory(originDirectory, backupDirectory);
                    return;
                }
                else
                {
                    tries--;
                    Console.WriteLine($"Incorrect path, please retry ({tries})");
                }
            }
            throw new DirectoryNotFoundException("Could not find the origin directory");
        }

        public static void SelectBackup()
        {
            //get all backup directories
            var backupDir = new DirectoryInfo(backupDirectory);
            var backups = backupDir.GetDirectories().Where(d => d.Name.Contains(Config.BackupFormat)).ToList();

            //print options in console
            Console.WriteLine($"Found {backups.Count} backups in {Config.BackupDirectory}");
            for (int i = 0; i < backups.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {backups[i].CreationTime}");
            }

            //ask for input
            bool shouldAsk = true;
            int retries = 3;
            int selected = -1;
            while (shouldAsk)
            {
                Console.Write("Select which backup to restore: ");
                string input = Console.ReadLine();

                try
                {
                    selected = Convert.ToInt32(input) - 1;
                    shouldAsk = false;
                }
                catch (FormatException)
                {
                    if (retries > 0)
                    {
                        Console.WriteLine("Could not convert the request, try again.");
                        retries--;
                    }
                    else
                        throw;
                }
            }

            //restore requested
            Restore(backups[selected].FullName);
        }

        public static void Restore(string backupPath)
        {
            //clear origin
            var originDirInfo = new DirectoryInfo(originDirectory);
            if (originDirInfo.Exists)
            {
                Console.WriteLine(originDirInfo.FullName);
            }
            //copy backup to the origin
        }
    }
}
