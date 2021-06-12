using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommonFunctionality
{
    public enum Operation
    {
        Backup = 0,
        Restore = 1,
        Rename = 2
    }

    public static class Config
    {
        private enum ConfigValue
        {
            //OriginDirectory,
            //BackupDirectory,
            BackupFormat,
            BackupMaxCount,
            LogFile,
            MemoryFile,
            MemoryDescription,
            DateTimeFormat,
            JarName,
            LaunchOpts,
            SelectedServerDir,
            SelectedBackupDir,
        }
        private static readonly Dictionary<ConfigValue, string> ConfigNames = new Dictionary<ConfigValue, string>
        {
            { ConfigValue.SelectedServerDir, "Selected Server Directory" },
            { ConfigValue.SelectedBackupDir, "Selected Backup Directory" },
            { ConfigValue.BackupFormat, "Backup Format" },
            { ConfigValue.BackupMaxCount, "Number of backups" },
            { ConfigValue.MemoryFile, "Memory File" },
            { ConfigValue.MemoryDescription, "Memory Format" },
            { ConfigValue.LogFile, "Log File" },
            { ConfigValue.DateTimeFormat, "DateTime Format" },
        };

        private static void WriteValue(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public static string SelectedServerDir
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.SelectedServerDir]);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            set
            {
                string key = ConfigNames[ConfigValue.SelectedServerDir];
                WriteValue(key, value);
            }
        }
        public static string SelectedBackupDir
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.SelectedBackupDir]);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            set
            {
                string key = ConfigNames[ConfigValue.SelectedBackupDir];
                WriteValue(key, value);
            }
        }

        public static string SelectedServerName
        {
            get
            {
                if (!Directory.Exists(SelectedServerDir)
                    || SelectedServerDir == string.Empty)
                    return NoServerSelected;

                string serverName = Path.GetFileName(SelectedServerDir);
                return $"Selected server: {serverName}";
            }
        }
        public static string BackupDirMessage
        {
            get
            {
                if (!Directory.Exists(SelectedBackupDir))
                {
                    return NoBackupSelected;
                }

                return SelectedBackupDir;
            }
        }

        public static string NoServerSelected => "No server selected";
        public static string NoBackupSelected => "Backup directory is not selected";


        public static string BackupFormat { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.BackupFormat]); }
        public static int BackupMaxCount => Convert.ToInt32(ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.BackupMaxCount]));

        public static string MemoryFile { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.MemoryFile]); }
        public static string MarkerFile => "marker";
        public static string MemoryDescription { get => ConfigurationManager.AppSettings.Get(ConfigNames[ConfigValue.MemoryDescription]); }


        public static string LogFilePath
        {
            get
            {
                string logPath = SelectedBackupDir;
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
        public static void CreateMemoryFile(string about, string where)
        {
            Directory.CreateDirectory(where);
            File.WriteAllText($"{where}\\backup{Config.MemoryFile}", $"{MemoryDescription}{about}");
        }
        public static string ReadMemoryFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not find the Memory file");

            return File.ReadAllLines(path).Where(l => l.StartsWith(Config.MemoryDescription)).FirstOrDefault().Substring(Config.MemoryDescription.Length);
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



        public delegate void RobocopyCompleteHandler(Operation operation, string destination);
        public delegate void RobocopyFailedHandler(Operation operation, Exception ex);

        public static event RobocopyCompleteHandler RobocopyComplete;
        public static event RobocopyFailedHandler RobocopyFailed;

        public static async Task RobocopyAsync(Operation operation, string source, string destination)
        {
            Process robocopy = new Process();
            robocopy.StartInfo.FileName = Config.RobocopyExe;
            robocopy.StartInfo.Arguments = $"\"{source}\" " + $"\"{destination}\" " + Config.RobocopyArgs;
            robocopy.StartInfo.CreateNoWindow = true;
            robocopy.StartInfo.UseShellExecute = false;
            bool res = false;
            res = await Task.Run(() =>
            {
                try
                {
                    //robocopy the source directory into new backup directory

                    robocopy.Start();
                    //robocopy.Exited += Robocopy_Exited;
                    return true;
                    //robocopy.WaitForExit();
                }
                catch (Exception ex)
                {
                    Directory.Delete(destination, true);

                    RobocopyFailed?.Invoke(operation, ex);
                    return false;
                }
            }, new CancellationTokenSource(new TimeSpan(0, 0, 30)).Token);

            if (res)
                RobocopyComplete?.Invoke(operation, destination);
            //if (res)
            //{
            //    await Task.Run(() =>
            //    {
            //        bool shouldCheck = !robocopy.HasExited;
            //        while (shouldCheck)
            //        {
            //            shouldCheck = !robocopy.HasExited;
            //        }
            //        RobocopyComplete?.Invoke(operation, destination);
            //    });
            //}
        }

        public static void Robocopy_Exited(object sender, EventArgs e)
        {
        }
    }
}
