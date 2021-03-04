using ServerManagerInterface.Models;
using System.Threading.Tasks;

namespace ServerManagerInterface.Controllers
{
    public class LockerWindowController
    {
        private readonly LockerWindowModel _model;
        private readonly InterfaceController _controller;

        public delegate void ControlsUnlockedHandler();
        public event ControlsUnlockedHandler ControlsUnlocked;

        public LockerWindowController(LockerWindowModel model, InterfaceController controller)
        {
            _model = model;
            _controller = controller;

            _model.UnlockMessageText = "Server is running.\nPress to unlock";
            _model.UnlockButtonText = "Unlock";
        }

        public Task UnlockInterface()
        {
            return Task.Run(async () =>
            {
                await _controller.SwitchStateAsync(State.Idle);
                ControlsUnlocked?.Invoke();
            });
        }
    }
}
