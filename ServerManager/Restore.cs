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
        private object _lock = new object();

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

        private async void Config_RobocopyComplete(Operation operation, string destination)
        {
            switch (operation)
            {
                case Operation.Restore:
                    {
                        ////clear origin
                        //if (Directory.Exists(Config.OriginDirectory))
                        //    Directory.Delete(Config.OriginDirectory, true);


                        //Directory.CreateDirectory(Config.OriginDirectory);
                        //await Config.RobocopyAsync(Operation.Rename, _temp_dir, Config.OriginDirectory);

                        //Directory.CreateDirectory(Config.OriginDirectory);
                        //rename temp dir to origin dir
                        //Directory.Move(_temp_dir, Config.OriginDirectory);
                        //Directory.Delete(_temp_dir, true);

                        //_temp_dir = string.Empty;


                        Complete?.Invoke(Config.OriginDirectory);
                        break;
                    }
                case Operation.Rename:
                    {
                        lock (_lock)
                        {

                            Directory.Delete(_temp_dir, true);
                            //_temp_dir = string.Empty;

                            Complete?.Invoke(Config.OriginDirectory);
                        }
                        break;
                    }
                default:
                    break;
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

                    //Directory.CreateDirectory(_temp_dir);

                    //copy backup to the temp dir
                    //await Config.RobocopyAsync(Operation.Restore, _selectedBackup, _temp_dir);
                    // 
                    //clear origin
                    if (Directory.Exists(Config.OriginDirectory))
                        Directory.Delete(Config.OriginDirectory, true);
                    await Config.RobocopyAsync(Operation.Restore, _selectedBackup, Config.OriginDirectory);
                }
                catch (Exception ex)
                {
                    //_temp_dir = string.Empty;
                    Failed?.Invoke(ex);
                    throw;
                }
            });
        }
    }
}
