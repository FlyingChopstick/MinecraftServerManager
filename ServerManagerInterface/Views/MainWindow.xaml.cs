using ServerManagerInterface.Controllers;
using System.Windows;


namespace ServerManagerInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InterfaceController _controller;
        private InterfaceModel _model;

        public MainWindow()
        {
            _model = new InterfaceModel();
            _controller = new InterfaceController(_model);

            _controller.ServerStarted += ServerStartedHandler;
            _controller.ServerStopped += ServerStoppedHandler;

            _controller.ControlsUpdated += ControlsUpdatedHander;

            this.DataContext = _model;


            InitializeComponent();
        }

        private void ControlsUpdatedHander()
        {
            UpdateControls();
        }

        private void ServerStoppedHandler()
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateControls();
            });
        }
        private void ServerStartedHandler()
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateControls();
            });
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            //await ServerBackup.ServerController.StartServerAsync();
            await _controller.StartServerAsync();
            //UpdateControls();
        }

        private void UpdateControls()
        {
            this.Dispatcher.Invoke(() =>
            {
                StartServer.IsEnabled = _model.ControlsEnabled;
                BackupServer.IsEnabled = _model.ControlsEnabled;
                RestoreServer.IsEnabled = _model.ControlsEnabled;

                StartServer.Content = _model.StartBtnContent;
                BackupServer.Content = _model.BackupBtnContent;
                RestoreServer.Content = _model.RestoreBtnContent;
            });
        }

        private async void BackupServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _controller.BackupServerAsync();
            }
            catch
            {
                _controller.BackupFailedNotif();
            }
        }


        private void RestoreServer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
