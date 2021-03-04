using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServerManagerInterface.Models
{
    public class LockerWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int MinWindowWidth
        {
            get => MinMessageWidth + 14;
        }
        public int MaxWindowWidth
        {
            get => MaxMessageWidth + 14;
        }

        public int MinMessageWidth { get; set; } = 100;
        public int MaxMessageWidth { get; set; } = 200;

        public int MinButtonWidth { get; set; } = 150;
        public int MaxButtonWidth { get; set; } = 250;


        public string UnlockMessageText { get; set; }
        public string UnlockButtonText { get; set; }
    }
}
