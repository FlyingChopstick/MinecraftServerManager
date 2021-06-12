using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommonFunctionality;
using Microsoft.Win32;
using ServerManager;

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

        private bool _isServerSelected;
        private bool _isBackupSelected;

        //public delegate void ServerStatusUpdHandler();
        //public delegate void ServerCrashedHandler(Exception ex);

        public delegate void ControlsUpdateHandler();
        public event ControlsUpdateHandler ControlsUpdated;

        public delegate void ServerStoppedDelegate();
        public event ServerStoppedDelegate ServerStopped;

        public InterfaceController(InterfaceModel model)
        {
            _model = model;


            _isServerSelected = Config.SelectedServerName != Config.NoServerSelected;
            _model.SelectedServerMessage = Config.SelectedServerName;


            _isBackupSelected = Config.BackupDirMessage != Config.NoBackupSelected;
            _model.BackupDirectoryMessage = Config.BackupDirMessage;


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


        /// <summary>
        /// Switches the state of the program
        /// </summary>
        /// <param name="newState"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the contents of buttons
        /// </summary>
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


        public static void SetBackupDirectory(string directoryPath)
        {
            Config.SelectedBackupDir = directoryPath;
        }
        public static void SetOriginDirectory(string directoryPath)
        {
            Config.SelectedServerDir = directoryPath;
        }
        public void SetTargetBackupDir(string backupDir)
        {
            _restore.SetTargetBackup(backupDir);
        }


        public bool ChangeActiveServer()
        {
            OpenFileDialog ofd = new();
            ofd.Filter = $"Marker files|*{Config.MarkerFile}";
            if (ofd.ShowDialog() == true)
            {
                ofd.Multiselect = false;
                string markerPath = ofd.FileName;
                string markerDir = Path.GetDirectoryName(markerPath);
                string markerDirName = Path.GetFileName(markerDir);

                Config.SelectedServerDir = markerDir;
                Config.SelectedServerDir = markerDir;
                _model.SelectedServerMessage = $"Selected server: {markerDirName}";
                _isServerSelected = true;
                ControlsUpdated?.Invoke();

                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ChangeBackupDirectory()
        {
            OpenFileDialog ofd = new();
            ofd.Filter = $"Marker files|*{Config.MarkerFile}";
            if (ofd.ShowDialog() == true)
            {
                ofd.Multiselect = false;
                string markerPath = ofd.FileName;
                string markerDir = Path.GetDirectoryName(markerPath);
                string markerDirName = Path.GetFileName(markerDir);

                Config.SelectedBackupDir = markerDir; ;
                _model.BackupDirectoryMessage = $"Backup directory: {markerDirName}";
                _isBackupSelected = true;
                ControlsUpdated?.Invoke();

                return true;
            }
            else
            {
                return false;
            }
        }


        bool CheckServerSelected()
        {
            if (!_isServerSelected)
            {
                MessageBox.Show(caption: "Server marker selection",
                    messageBoxText: "Server marker is not selected. Please tell where the server is located.",
                    icon: MessageBoxImage.Information,
                    button: MessageBoxButton.OK);


                if (!ChangeActiveServer())
                    //TODO display error message
                    return false;
            }
            return true;
        }
        bool CheckBackupDirSelected()
        {
            if (!_isBackupSelected)
            {
                MessageBox.Show(caption: "Backup marker selection",
                    messageBoxText: "Backup marker is not selected. Please tell where the backup directory is located.",
                    icon: MessageBoxImage.Information,
                    button: MessageBoxButton.OK);


                if (!ChangeBackupDirectory())
                    //TODO display error message
                    return false;
            }
            return true;
        }


        private Task StartServerAsync()
        {
            if (!CheckServerSelected())
            {
                return SwitchStateAsync(State.Idle);
            }
            return _start.StartServerAsync();
        }
        private async Task BackupServerAsync()
        {
            if (!CheckServerSelected()
                || !CheckBackupDirSelected())
            {
                await SwitchStateAsync(State.Idle);
            }
            await _backup.BackupServerAsync();
            await SwitchStateAsync(State.Idle);
        }
        private Task RestoreBackupAsync()
        {
            if (!CheckServerSelected())
            {
                return SwitchStateAsync(State.Idle);
            }
            return _restore.RestoreBackupAsync();
        }


        private async void ServerStoppedHandlerAsync()
        {
            ServerStopped?.Invoke();
            await SwitchStateAsync(State.Idle);
        }
        private void ServerCrashedHandlerAsync(Exception ex)
        {
            MessageBox.Show(
                caption: "Server crashed",
                messageBoxText: $"Server crashed due to exception: {ex.Message}",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
                );
            //await SwitchStateAsync(State.Idle);
            ServerStoppedHandlerAsync();
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
