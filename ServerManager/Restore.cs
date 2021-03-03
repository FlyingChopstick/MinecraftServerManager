using CommonFunctionality;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerManager
{
    public class Restore
    {
        //private string _originDirectory = Config.SelectedServerDir;
        //private string _backupDirectory = Config.SelectedBackupDir;
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

        private void Config_RobocopyComplete(Operation operation, string destination)
        {
            switch (operation)
            {
                case Operation.Restore:
                    {
                        ////clear origin
                        //if (Directory.Exists(Config.SelectedServerDir))
                        //    Directory.Delete(Config.SelectedServerDir, true);


                        //Directory.CreateDirectory(Config.SelectedServerDir);
                        //await Config.RobocopyAsync(Operation.Rename, _temp_dir, Config.SelectedServerDir);

                        //Directory.CreateDirectory(Config.SelectedServerDir);
                        //rename temp dir to origin dir
                        //Directory.Move(_temp_dir, Config.SelectedServerDir);
                        //Directory.Delete(_temp_dir, true);

                        //_temp_dir = string.Empty;


                        Complete?.Invoke(Config.SelectedServerDir);
                        break;
                    }
                case Operation.Rename:
                    {
                        lock (_lock)
                        {

                            Directory.Delete(_temp_dir, true);
                            //_temp_dir = string.Empty;

                            Complete?.Invoke(Config.SelectedServerDir);
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
                    _temp_dir = $"{Config.SelectedServerDir}_temp";
                    if (Directory.Exists(_temp_dir))
                        Directory.Delete(_temp_dir, true);

                    //Directory.CreateDirectory(_temp_dir);

                    //copy backup to the temp dir
                    //await Config.RobocopyAsync(Operation.Restore, _selectedBackup, _temp_dir);
                    // 
                    //clear origin
                    if (Directory.Exists(Config.SelectedServerDir))
                        Directory.Delete(Config.SelectedServerDir, true);
                    await Config.RobocopyAsync(Operation.Restore, _selectedBackup, Config.SelectedServerDir);
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
