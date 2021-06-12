using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonFunctionality
{
    public enum MarkerType
    {
        Server = 0,
        Backup = 1
    }

    public class Marker
    {
        public Marker(MarkerType type, string filePath)
        {
            FileContract = new(filePath);

            if (type == MarkerType.Server)
            {
                if (ServerMarkerExists())
                {
                    ParseMarker();
                }
            }
        }
        public static Marker ForDirectory(MarkerType type, string directory)
        {
            var markerFile = Directory.GetFiles(directory).Where(f => f.Contains(Config.MarkerFile)).FirstOrDefault();
            if (markerFile == default)
            {
                return new(type, $"{directory}\\{type}.{Config.MarkerFile}");
            }
            else
            {
                return new(type, markerFile);
            }
        }



        public string FilePath => FileContract.FilePath;
        public string Filename => FileContract.FileName;
        public FileContract FileContract { get; private set; }


        private bool ServerMarkerExists()
        {
            if (FileContract.Exists)
            {
                return true;
            }

            FileContract.Append(DefaultSettings);
            return false;
        }
        public void ParseMarker()
        {
            if (FileContract.Exists)
            {
                List<string> text = new(File.ReadAllLines(FileContract.FilePath));

                var javaPathLine = text.Find(l => l.Contains(ConfigCustomJRE));

                if (IsModified(javaPathLine, ConfigCustomJRE))
                {
                    Config.JavaPath = Path.GetFullPath(javaPathLine[(ConfigCustomJRE.Length + 3)..]);
                }
                else
                {
                    Config.JavaPath = "java";
                }

                Config.LaunchOpts = string.Empty;

                var xms = text.Find(l => l.Contains(ConfigXms));
                if (IsModified(xms, ConfigXms))
                {
                    Config.LaunchOpts += $"-Xms{xms[(ConfigXms.Length + 3)..]} ";
                }
                else
                {
                    Config.LaunchOpts += $"-Xms{Requirements[ConfigXms]} ";
                }


                var xmx = text.Find(l => l.Contains(ConfigXmx));
                if (IsModified(xmx, ConfigXmx))
                {
                    Config.LaunchOpts += $"-Xms{xms[(ConfigXmx.Length + 3)..]} ";
                }
                else
                {
                    Config.LaunchOpts += $"-Xms{Requirements[ConfigXmx]} ";
                }

                var extra = text.Find(l => l.Contains(ConfigExtraArgs));
                if (IsModified(extra, ConfigExtraArgs))
                {
                    Config.LaunchOpts += extra[(ConfigExtraArgs.Length + 3)..];
                }
            }
        }


        private static bool IsModified(string line, string argument)
        {
            return line != default
                && !line.Contains(ConfigEmptyValue);
        }

        private List<string> DefaultSettings
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
        private readonly Dictionary<string, string> Requirements = new()
        {
            { ConfigCustomJRE, ConfigEmptyValue },
            { ConfigXms, "2G" },
            { ConfigXmx, "4G" },
            { ConfigExtraArgs, ConfigEmptyValue },
        };

        private static readonly string ConfigEmptyValue = "none";
        private static readonly string ConfigCustomJRE = "Custom JRE";
        private static readonly string ConfigXms = "Xms";
        private static readonly string ConfigXmx = "Xmx";
        private static readonly string ConfigExtraArgs = "Additional arguments";
    }
}
