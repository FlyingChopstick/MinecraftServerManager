using System.Threading.Tasks;
using System.Windows;

namespace ServerManagerInterface.Controllers
{
    public class InterfaceController
    {
        private readonly InterfaceModel _model;

        public delegate void ServerStatusUpdHandler();
        public event ServerStatusUpdHandler ServerStarted;
        public event ServerStatusUpdHandler ServerStopped;

        public delegate void ControlsUpdateHandler();
        public event ControlsUpdateHandler ControlsUpdated;

        public InterfaceController(InterfaceModel model)
        {
            ServerBackup.ServerController.ServerStarted += ServerController_ServerStarted;
            ServerBackup.ServerController.ServerStopped += ServerController_ServerStopped;

            ServerBackup.ServerController.BackupStarted += ServerController_BackupStarted;
            ServerBackup.ServerController.BackupComplete += ServerController_BackupComplete;
            ServerBackup.ServerController.BackupFailed += ServerController_BackupFailed;
            _model = model;
        }

        private string _lastBackupPath = string.Empty;
        private bool backupCompleteOrFailed = false;

        public void ToggleControls(bool enable)
        {
            _model.ControlsEnabled = enable;
        }

        public void ServerController_ServerStarted()
        {
            ToggleControls(false);
            _model.StartBtnContent = "Running";
            ControlsUpdated?.Invoke();

            ServerStarted?.Invoke();
        }
        public void ServerController_ServerStopped()
        {
            _model.StartBtnContent = "Start Server";
            ToggleControls(true);
            ControlsUpdated?.Invoke();

            ServerStopped?.Invoke();
        }



        private void ServerController_BackupStarted()
        {
            ToggleControls(false);
            _model.BackupBtnContent = "Backing up...";
            ControlsUpdated?.Invoke();
        }
        private void ServerController_BackupComplete(string backupPath)
        {
            MessageBox.Show(
                caption: "Backup Success",
                messageBoxText: $"Successfully backed up to {backupPath}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information
                );

            _model.BackupBtnContent = "Backup";
            ToggleControls(true);
            ControlsUpdated?.Invoke();
        }
        private void ServerController_BackupFailed()
        {
            backupCompleteOrFailed = true;
            BackupFailedNotif();
        }

        public void BackupFailedNotif()
        {
            MessageBox.Show(
                caption: "Backup Failed",
                messageBoxText: "Could not backup the server",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
                );

            _model.BackupBtnContent = "Backup";
            ToggleControls(true);
            ControlsUpdated?.Invoke();
        }



        public async Task StartServerAsync()
        {
            await ServerBackup.ServerController.StartServerAsync();
        }
        public async Task BackupServerAsync()
        {
            _lastBackupPath = await ServerBackup.ServerController.BackupServerAsync();
        }
    }
}
