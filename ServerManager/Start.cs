using System;
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

        //private Process _server;
        //private Server server;

        public Start()
        {
            Server.Exited += Server_Exited;
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

                var marker = Marker.ForDirectory(MarkerType.Server, Config.SelectedServerDir);

                Server.Directory = marker.ServerDir;
                Server.Arguments = "/C " +
                marker.JavaPath +
                " " +
                marker.LaunchArgs +
                $" -jar " +
                marker.ServerJar +
                " " +
                marker.Gui;

                Server.Exited += Server_Exited;
                Server.Start();
            });
        }

        private void Server_Exited(object sender, EventArgs e)
        {
            Stopped?.Invoke();
        }
    }
}
