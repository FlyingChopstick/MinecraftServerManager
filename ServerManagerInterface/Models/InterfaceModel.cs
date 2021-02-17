using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServerManagerInterface
{
    public class InterfaceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int ButtonMinWidth { get; set; } = 150;

        public bool ControlsEnabled { get; set; } = true;

        public string StartBtnContent { get; set; }
        public string BackupBtnContent { get; set; }
        public string RestoreBtnContent { get; set; }

    }
}
