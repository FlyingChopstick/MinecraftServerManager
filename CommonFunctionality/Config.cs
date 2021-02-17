using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CommonFunctionality
{

    public static class Config
    {
        private enum ConfigValue
        {
            OriginDirectory,
            BackupDirectory,
            BackupFormat,
            BackupMaxCount,
            LogFile,
            MemoryFile,
            MemoryDescription,
            DateTimeFormat
        }
        private static readonly Dictionary<ConfigValue, string> ConfigNames = new Dictionary<ConfigValue, string>
        {
            { ConfigValue.OriginDirectory, "Origin Directory" },
            { ConfigValue.BackupDirectory, "Backup Directory" },
            { ConfigValue.BackupFormat, "Backup Format" },
            { ConfigValue.BackupMaxCount, "Number of backups" },
            { ConfigValue.LogFile, "Log File" },
            { ConfigValue.MemoryFile, "Memory File" },
            { ConfigValue.MemoryDescription, "Memory Format" },
            { ConfigValue.DateTimeFormat, "DateTime Format" },
        };




        public static string OriginDirectory => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.OriginDirectory]);


        public static string BackupDirectory { get => ConfigurationManager.AppSettings.Get("Backup Directory"); }
        public static string BackupFormat { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.BackupFormat]); }
        public static int BackupMaxCount => Convert.ToInt32(ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.BackupMaxCount]));

        public static string MemoryFile { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.MemoryFile]); }
        public static string MemoryDescription { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.MemoryDescription]); }


        public static string LogFilePath
        {
            get
            {
                string logPath = BackupDirectory;
                string logName = ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.LogFile]);
                return $"{logPath}\\{logName}";
            }
        }
        public static string DateTimeFormat => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.DateTimeFormat]);


        public static string RobocopyExe { get => @"C:\Windows\System32\Robocopy.exe"; }
        public static string RobocopyArgs { get => "/E /it"; }


        /// <summary>
        /// Creates a Memory file describing the path to the origin folder
        /// </summary>
        /// <param name="about">Path to the origin folder</param>
        /// <param name="where">Where to create a Memory file</param>
        public static void CreateMemory(string about, string where)
        {
            Directory.CreateDirectory(where);
            File.WriteAllText($"{where}\\{Config.MemoryFile}", $"{MemoryDescription}{about}");
        }
        public static void Robocopy(string source, string destination)
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
                //robocopy.WaitForExit();
            }
            catch (Exception)
            {
                Directory.Delete(destination, true);
                throw;
            }
        }

        public static Task RobocopyAsync(string source, string destination)
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
                    //robocopy.WaitForExit();
                }
                catch (Exception)
                {
                    Directory.Delete(destination, true);
                    throw;
                }
            });
        }
    }
}
