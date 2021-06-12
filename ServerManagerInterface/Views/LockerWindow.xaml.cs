using System;
using System.Windows;
using ServerManagerInterface.Controllers;
using ServerManagerInterface.Models;

namespace ServerManagerInterface.Views
{
    /// <summary>
    /// Interaction logic for LockerWindow.xaml
    /// </summary>
    public partial class LockerWindow : Window
    {
        readonly LockerWindowModel _model;
        readonly LockerWindowController _controller;

        readonly Window _parent;

        public LockerWindow(InterfaceController parentController)
        {
            _model = new();
            _controller = new(_model, parentController);

            parentController.ServerStopped += ParentController_ServerStopped;

            _parent = Application.Current.MainWindow;
            _parent.StateChanged += ParentWindowStateChanged;

            this.DataContext = _model;
            _controller.ControlsUnlocked += Controller_ControlsUnlocked;

            InitializeComponent();
            UpdateControls();
        }

        private void ParentWindowStateChanged(object sender, System.EventArgs e)
        {
            this.WindowState = _parent.WindowState;
        }
        protected override void OnStateChanged(EventArgs e)
        {
            _parent.WindowState = this.WindowState;
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
