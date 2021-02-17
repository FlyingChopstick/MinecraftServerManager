using CommonFunctionality;
using ServerManager;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ServerManagerInterface.Controllers
{
    public enum State
    {
        Idle = 0,
        Running = 1,
        Backup = 2,
        Restore = 3
    }
    public class InterfaceController
    {

        private State _state;

        private readonly InterfaceModel _model;

        private readonly Start _start;
        private readonly Backup _backup;
        private readonly Restore _restore;

        //public delegate void ServerStatusUpdHandler();
        //public delegate void ServerCrashedHandler(Exception ex);

        public delegate void ControlsUpdateHandler();
        public event ControlsUpdateHandler ControlsUpdated;

        public InterfaceController(InterfaceModel model)
        {
            _model = model;
            _state = State.Idle;

            _start = new();
            _backup = new();
            _restore = new();

            _start.Stopped += ServerStoppedHandlerAsync;
            _start.Crashed += ServerCrashedHandlerAsync;

            _backup.Complete += BackupCompleteHandlerAsync;
            _backup.Failed += BackupFailedHandlerAsync;

            _restore.Complete += RestoreCompleteHandlerAsync;
            _restore.Failed += RestoreFailedHandlerAsync;

        }



        public async Task SwitchStateAsync(State newState)
        {
            _state = newState;
            //update controls
            UpdateControls();
            //refresh controls
            ControlsUpdated?.Invoke();
            //start operations
            switch (_state)
            {
                case State.Idle:
                    break;
                case State.Running:
                    await StartServerAsync();
                    break;
                case State.Backup:
                    await BackupServerAsync();
                    break;
                case State.Restore:
                    await RestoreBackupAsync();
                    break;
                default:
                    break;
            }
        }
        public void UpdateControls()
        {
            switch (_state)
            {
                case State.Idle:
                    _model.StartBtnContent = "Start Server";
                    _model.BackupBtnContent = "Backup Server";
                    _model.RestoreBtnContent = "Restore Server";
                    _model.ControlsEnabled = true;
                    break;
                case State.Running:
                    _model.StartBtnContent = "Running...";
                    _model.BackupBtnContent = "Backup Server";
                    _model.RestoreBtnContent = "Restore Server";
                    _model.ControlsEnabled = false;
                    break;
                case State.Backup:
                    _model.StartBtnContent = "Start Server";
                    _model.BackupBtnContent = "Backing up...";
                    _model.RestoreBtnContent = "Restore Server";
                    _model.ControlsEnabled = false;
                    break;
                case State.Restore:
                    _model.StartBtnContent = "Start Server";
                    _model.BackupBtnContent = "Backup Server";
                    _model.RestoreBtnContent = "Restoring...";
                    _model.ControlsEnabled = false;
                    break;
                default:
                    break;
            }
        }

        public void SetBackupDirectory(string directoryPath)
        {
            Config.BackupDirectory = directoryPath;
        }
        public void SetOriginDirectory(string directoryPath)
        {
            Config.OriginDirectory = directoryPath;
        }
        public void SetTargetBackupDir(string backupDir)
        {
            _restore.SetTargetBackup(backupDir);
        }


        private async Task StartServerAsync()
        {
            await _start.StartServerAsync();
        }
        private async Task BackupServerAsync()
        {
            await _backup.BackupServerAsync();
        }
        private async Task RestoreBackupAsync()
        {
            await _restore.RestoreBackupAsync();
        }


        private async void ServerStoppedHandlerAsync()
        {
            await SwitchStateAsync(State.Idle);
        }
        private async void ServerCrashedHandlerAsync(Exception ex)
        {
            MessageBox.Show(
                caption: "Server crashed",
                messageBoxText: $"Server crashed due to exception: {ex.Message}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
                );
            await SwitchStateAsync(State.Idle);
        }

        private async void BackupCompleteHandlerAsync(string backupPath)
        {
            MessageBox.Show(
                caption: "Successful backup",
                messageBoxText: $"Successfully backed up to {backupPath}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information
                );

            await SwitchStateAsync(State.Idle);
        }
        private async void BackupFailedHandlerAsync(Exception ex)
        {
            CouldNot("Backup failed", "backup the server", ex);
            await SwitchStateAsync(State.Idle);
        }

        private async void RestoreCompleteHandlerAsync(string backupName)
        {
            MessageBox.Show(
                caption: "Backup Restored",
                messageBoxText: $"Successfully restored backup {backupName}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information
                );

            await SwitchStateAsync(State.Idle);
        }
        private async void RestoreFailedHandlerAsync(Exception ex)
        {
            CouldNot("Restore failed", "restore backup", ex);
            await SwitchStateAsync(State.Idle);
        }


        /// <summary>
        /// Displays a message box describing an exception
        /// </summary>
        /// <param name="title"></param>
        /// <param name="couldNot"></param>
        /// <param name="ex"></param>
        private static void CouldNot(string title, string couldNot, Exception ex)
        {
            MessageBox.Show(
                caption: title,
                messageBoxText: $"Could not {couldNot} due to exception: {ex}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
                );
        }
    }
}
