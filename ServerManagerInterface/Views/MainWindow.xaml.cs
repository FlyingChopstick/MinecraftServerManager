using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using CommonFunctionality;
using Microsoft.Win32;
using ServerManagerInterface.Controllers;
using ServerManagerInterface.Views;

namespace ServerManagerInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InterfaceController _controller;
        private readonly InterfaceModel _model;
        private readonly List<DirectoryInfo> _backupList;
        //private Window _lockerWindow;

        public MainWindow()
        {
            _model = new();
            _controller = new(_model);
            _backupList = new();

            _controller.ControlsUpdated += ControlsUpdatedHander;


            this.DataContext = _model;

            InitializeComponent();

            UpdateControls();
        }

        private void ControlsUpdatedHander()
        {
            RefreshControls();
        }
        private void RefreshControls()
        {
            this.Dispatcher.Invoke(() =>
            {
                StartServer.IsEnabled = _model.ControlsEnabled;
                BackupServer.IsEnabled = _model.ControlsEnabled;
                RestoreServer.IsEnabled = _model.ControlsEnabled;

                StartServer.Content = _model.StartBtnContent;
                SelectedServer.Text = _model.SelectedServerMessage;
                BackupDirectory.Text = _model.BackupDirectoryMessage;

                BackupServer.Content = _model.BackupBtnContent;
                RestoreServer.Content = _model.RestoreBtnContent;
            });
        }
        private void UpdateControls()
        {
            _controller.UpdateControls();
            RefreshControls();
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            var lv = new LockerWindow(_controller);
            lv.Show();

            await _controller.SwitchStateAsync(State.Running);
        }
        private async void BackupServer_Click(object sender, RoutedEventArgs e)
        {
            await _controller.SwitchStateAsync(State.Backup);
        }
        private void RestoreServer_Click(object sender, RoutedEventArgs e)
        {
            if (IsMemorySelected())
            {
                ShowBackupList();
            }
        }


        private static bool IsMemorySelected()
        {
            OpenFileDialog ofd = new();
            ofd.Filter = $"Memory Files|*{Config.MemoryFile}";
            if (ofd.ShowDialog() == true)
            {
                ofd.Multiselect = false;
                string memoryPath = ofd.FileName;
                try
                {
                    string originDiretory = Config.ReadMemoryFile(memoryPath);
                    string backupDirectory = Path.GetDirectoryName(memoryPath);

                    InterfaceController.SetBackupDirectory(backupDirectory);
                    InterfaceController.SetOriginDirectory(originDiretory);
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show($"Could not read the Memory file; Restore failed: {ex}");
                }
                return true;
            }

            return false;
        }
        private void ShowBackupList()
        {
            _backupList.Clear();
            var backups = Directory.GetDirectories(Config.SelectedBackupDir).ToList();
            if (backups.Count == 0)
            {
                MessageBox.Show("Could not find any backups");
                return;
            }

            ListBorder.Visibility = Visibility.Visible;

            foreach (var dir in backups)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                _backupList.Add(di);
                BackupList.Items.Add(di.Name);
            }
        }

        private async void BackupList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int selectedIndex = BackupList.SelectedIndex;
            _controller.SetTargetBackupDir(_backupList[selectedIndex].FullName);

            _backupList.Clear();
            BackupList.Items.Clear();
            ListBorder.Visibility = Visibility.Collapsed;

            await _controller.SwitchStateAsync(State.Restore);
        }

        private void SelectedServer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _controller.ChangeActiveServer();
        }
        private void BackupDirectory_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _controller.ChangeBackupDirectory();
        }
    }
}
