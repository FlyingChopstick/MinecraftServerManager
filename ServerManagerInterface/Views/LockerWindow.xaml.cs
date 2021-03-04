using ServerManagerInterface.Controllers;
using ServerManagerInterface.Models;
using System.Threading.Tasks;
using System.Windows;

namespace ServerManagerInterface.Views
{
    /// <summary>
    /// Interaction logic for LockerWindow.xaml
    /// </summary>
    public partial class LockerWindow : Window
    {
        readonly LockerWindowModel _model;
        readonly LockerWindowController _controller;

        public LockerWindow(InterfaceController parentController)
        {
            _model = new();
            _controller = new(_model, parentController);

            parentController.ServerStopped += ParentController_ServerStopped;

            this.DataContext = _model;
            _controller.ControlsUnlocked += Controller_ControlsUnlocked;

            InitializeComponent();
            UpdateControls();
        }

        private void UpdateControls()
        {
            this.Dispatcher.Invoke(() =>
            {
                RunningMessage.Text = _model.UnlockMessageText;
                UnlockButton.Content = _model.UnlockButtonText;
            });
        }

        private void ParentController_ServerStopped()
        {
            this.Dispatcher.Invoke(() =>
            {
                Close();
            });
        }

        private void Controller_ControlsUnlocked()
        {
            this.Dispatcher.Invoke(() =>
            {
                Close();
            });
        }

        private async void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            await _controller.UnlockInterface();
        }
    }
}
