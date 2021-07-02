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
        public delegate void EulaNotAcceptedHandler();
        public event ServerStatusHandler Stopped;
        public event ServerCrashHandler Crashed;
        public event EulaNotAcceptedHandler OnEulaCrash;

        public void AcceptEula()
        {
            var marker = Marker.ForDirectory(MarkerType.Server, Config.SelectedServerDir);
            //marker.IsEulaAccepted = true;
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
                var server = new Server(marker)
                {
                    //server.Directory = marker.ServerDir;
                    Arguments = "/C " +
                        marker.JavaPath +
                        " " +
                        marker.LaunchArgs +
                        $" -jar " +
                        marker.ServerJar +
                        " " +
                        marker.Gui
                };

                server.OnServerExited += Server_OnServerExited;
                server.Start();
            });
        }

        private void Server_OnServerExited(bool isEulaAccepted = true)
        {
            if (!isEulaAccepted)
            {
                OnEulaCrash?.Invoke();
                return;
            }

            Stopped?.Invoke();
        }
    }
}
