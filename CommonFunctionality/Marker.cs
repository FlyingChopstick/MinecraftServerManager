﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonFunctionality
{
    public enum MarkerType
    {
        Server = 0,
        Backup = 1
    }

    static class MarkerConf
    {
        public static List<string> DefaultSettings
        {
            get
            {
                var strings = new List<string>();

                foreach (var req in Requirements.Keys)
                {
                    strings.Add($"{req} = {Requirements[req]}");
                }

                return strings;
            }
        }
        public static readonly Dictionary<string, string> Requirements = new()
        {
            { JavaPath, EmptyValue },
            { ShowGui, BoolFalse },
            //{ IsEulaAccepted, BoolFalse },
            { LaunchArgs, "-Xms2G -Xmx4G" },
        };

        public const string EmptyValue = "none";
        public const string BoolTrue = "true";
        public const string BoolFalse = "false";


        public const string JavaPath = "Custom JRE";
        public const string ShowGui = "Show GUI";
        //public const string IsEulaAccepted = "User accepted EULA";
        public const string LaunchArgs = "Launch arguments";
    }

    public class Marker
    {
        public Marker(MarkerType type, string filePath)
        {
            //ServerDir = Path.GetDirectoryName(filePath);
            FileContract = new(filePath);

            switch (type)
            {
                case MarkerType.Server:
                    {
                        SetupServerMarker();
                    }
                    break;
                case MarkerType.Backup:
                    {
                        SetupBackupMarker();
                    }
                    break;
                default:
                    break;
            }
            Type = type;



            string eulaFile = $"{ServerDir}\\eula.txt";
            IsEulaAccepted = File.Exists(eulaFile)
                && File.ReadAllText(eulaFile).Contains("eula=true");
        }
        public static Marker ForDirectory(MarkerType type, string directory)
        {
            var markerFile = Directory.GetFiles(directory).Where(f => f.Contains(Config.MarkerFile)).FirstOrDefault();
            if (markerFile == default)
            {
                return new(type, $"{directory}\\{type}.{Config.MarkerFile}");
            }

            return new(type, markerFile);
        }



        public string MarkerPath => FileContract.FilePath;
        public string MarkerName => FileContract.FileName;
        public string ServerDir => FileContract.Directory;
        public string ServerJar => Path.GetFileName(
            Directory.GetFiles(ServerDir)
            .Where(f => f.EndsWith(".jar"))
            .FirstOrDefault());
        public string JavaPath => ParseConfig(MarkerConf.JavaPath, "java");
        public string LaunchArgs => ParseConfig(MarkerConf.LaunchArgs);
        public string Gui => ParseConfig(MarkerConf.ShowGui) == MarkerConf.BoolTrue ? "" : "nogui";

        public bool IsEulaAccepted
        {
            get;
            //{
            //    return ParseConfig(MarkerConf.IsEulaAccepted) == MarkerConf.BoolTrue;
            //}
            //set
            //{
            //    FileContract.UpdateLine(MarkerConf.IsEulaAccepted, value ?
            //        $"{MarkerConf.IsEulaAccepted} = {MarkerConf.BoolTrue}"
            //        : $"{MarkerConf.IsEulaAccepted} = {MarkerConf.BoolFalse}");
            //}
        }


        public FileContract FileContract { get; }
        public MarkerType Type { get; }


        private List<string> MarkerSource = new();


        private void SetupServerMarker()
        {
            if (!FileContract.Exists)
            {
                FileContract.Append(MarkerConf.DefaultSettings);
                return;
            }

            MarkerSource = FileContract.Read();
        }
        private void SetupBackupMarker()
        {
            if (!FileContract.Exists)
            {
                FileContract.CreateFile();
            }
        }
        private string ParseConfig(string configName, string fallback = "")
        {
            var configLine = MarkerSource.Find(l => l.StartsWith(configName));
            if (IsModified(configLine, configName))
            {
                return configLine[(configName.Length + 3)..];
            }

            return fallback;
        }

        private static bool IsModified(string line, string configName)
        {
            return line != default
                && line.StartsWith(configName)
                && !line.EndsWith(MarkerConf.EmptyValue);
        }
    }
}
