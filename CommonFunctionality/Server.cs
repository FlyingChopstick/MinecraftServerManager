using System;
using System.Diagnostics;

namespace CommonFunctionality
{
    public class Server : IDisposable
    {

        public delegate void ServerExitedHandler(bool isEulaAccepted = true);
        public event ServerExitedHandler OnServerExited;

        public Server(Marker serverMarker)
        {
            Directory = serverMarker.ServerDir;
            IsEulaAccepted = serverMarker.IsEulaAccepted;
        }

        public void Start(bool enableRaisingEvents = true)
        {
            _server = new();

            _server.EnableRaisingEvents = enableRaisingEvents;

            _server.StartInfo.FileName = "cmd";
            _server.StartInfo.WorkingDirectory = Directory;
            _server.StartInfo.Arguments = Arguments;

            _server.Start();
            _server.Exited += ServerExited; ;
        }
        public void Shutdown()
        {
            _server.Kill();
        }

        private void ServerExited(object sender, EventArgs e)
        {
            OnServerExited?.Invoke(IsEulaAccepted);
        }

        public string Arguments { get; set; } = string.Empty;
        public string Directory { get; init; } = string.Empty;
        public bool IsEulaAccepted { get; }

        //public event EventHandler Exited;
        private Process _server;
        private bool _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _server.Kill();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
