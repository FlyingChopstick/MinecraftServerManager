using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommonFunctionality;

namespace ServerManager
{
    public class Start
    {
        public delegate void ServerStatusHandler();
        public delegate void ServerCrashHandler(Exception ex);
        public event ServerStatusHandler Stopped;
        public event ServerCrashHandler Crashed;

        private Process _server;

        public Start()
        {

        }

        public Task StartServerAsync()
        {

            return Task.Run(() =>
            {
                if (!Directory.Exists(Config.SelectedServerDir))
                {
                    DirectoryNotFoundException ex = new("Could not find the server directory, closing.");
                    Crashed?.Invoke(ex);
                    return;
                }

                Directory.SetCurrentDirectory(Config.SelectedServerDir);

                //Config.CreateMemoryFile(Config.SelectedServerDir, Config.SelectedBackupDir);

                //launch the server
                _server = new Process();

                _server.StartInfo.WorkingDirectory = Config.SelectedServerDir;

                _server.StartInfo.FileName = "cmd";

                string javaPath = Config.IsJava15Required ? Config.Java15Path : "java";

                _server.StartInfo.Arguments = "/C " +
                javaPath +
                " " + //"java -d64 " +
                $"{Config.LaunchOpts} " +
                $"-jar {Config.ServerJar} nogui";

                _server.EnableRaisingEvents = true;
                _server.Start();
                _server.Exited += Server_Exited;
            });
        }

        private void Server_Exited(object sender, EventArgs e)
        {
            Stopped?.Invoke();
        }
    }
}
