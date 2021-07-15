using System.Linq;
using System.Windows;
using ServerManager;

namespace ServerManagerInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void StartupAsync(object sender, StartupEventArgs e)
        {
            if (e.Args.ToList().Contains("-s"))
            {
                Start start = new();
                start.Stopped += ServerStoppedHandler;
                await start.StartServerAsync();

            }

            //Backup backup = new();
            //backup.Failed += Backup_Failed;
            //backup.Complete += Backup_Complete;
            //await backup.BackupServerAsync();
        }

        private async void ServerStoppedHandler()
        {
            Backup backup = new();
            backup.Complete += BackupCompleteHandler;
            await backup.BackupServerAsync();
        }

        private void BackupCompleteHandler(string path)
        {
            this.MainWindow.Close();
        }
    }
}
