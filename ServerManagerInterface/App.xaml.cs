using System;
using System.Windows;
using ServerManager;

namespace ServerManagerInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void StartupAsync(object sender, StartupEventArgs e)
        {
            Backup backup = new();
            backup.Failed += Backup_Failed;
            backup.Complete += Backup_Complete;
            //await backup.BackupServerAsync();
        }

        private async void Backup_Complete(string path)
        {
            //Start start = new();
            //await start.StartServerAsync();
        }

        private void Backup_Failed(Exception ex)
        {
            MessageBox.Show(messageBoxText: "Could not backup the server",
                caption: "Backup failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
