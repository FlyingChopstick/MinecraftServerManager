using System;
using System.Diagnostics;

namespace CommonFunctionality
{
    public static class Server
    {
        public static void Start(bool enableRaisingEvents = true)
        {
            _server = new();
            _server.EnableRaisingEvents = enableRaisingEvents;
            _server.StartInfo.FileName = "cmd";
            _server.StartInfo.WorkingDirectory = Directory;
            _server.StartInfo.Arguments = Arguments;

            _server.Start();
            _server.Exited += Exited;
        }

        public static string Arguments { get; set; } = string.Empty;
        public static string Directory { get; set; } = string.Empty;

        public static event EventHandler Exited;
        private static Process _server;
    }
}
