using CommonFunctionality;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerManager
{
    public class Restore
    {
        //private string _originDirectory = Config.OriginDirectory;
        //private string _backupDirectory = Config.BackupDirectory;
        private string _selectedBackup = string.Empty;
        private string _temp_dir = string.Empty;

        public delegate void RestoreStartedHandler();
        public delegate void RestoreCompletedHandler(string backupName);
        public delegate void RestoreFailedHandler(Exception ex);

        public event RestoreCompletedHandler Complete;
        public event RestoreFailedHandler Failed;


        public Restore()
        {
            Config.RobocopyComplete += Config_RobocopyComplete;
            Config.RobocopyFailed += Config_RobocopyFailed;
        }

        private void Config_RobocopyComplete(Operation operation, string destination)
        {
            if (operation == Operation.Restore)
            {
                //clear origin
                if (Directory.Exists(Config.OriginDirectory))
                    Directory.Delete(Config.OriginDirectory, true);

                //rename temp dir to origin dir
                Directory.Move(_temp_dir, Config.OriginDirectory);

                _temp_dir = string.Empty;

                Complete?.Invoke(destination);
            }
        }
        private void Config_RobocopyFailed(Operation operation, Exception ex)
        {
            if (operation == Operation.Restore)
            {
                Failed?.Invoke(ex);
            }
        }

        public void SetTargetBackup(string dirPath)
        {
            _selectedBackup = dirPath;
        }
        public Task RestoreBackupAsync()
        {
            return Task.Run(async () =>
            {
                try
                {
                    //create temp dir
                    _temp_dir = $"{Config.OriginDirectory}_temp";
                    if (Directory.Exists(_temp_dir))
                        Directory.Delete(_temp_dir, true);

                    Directory.CreateDirectory(_temp_dir);

                    //copy backup to the temp dir
                    Config.RobocopyComplete += Config_RobocopyComplete;
                    Config.RobocopyFailed += Config_RobocopyFailed;
                    await Config.RobocopyAsync(Operation.Restore, _selectedBackup, _temp_dir);
                }
                catch (Exception ex)
                {
                    _temp_dir = string.Empty;
                    Failed?.Invoke(ex);
                    throw;
                }
            });
        }
    }
}
