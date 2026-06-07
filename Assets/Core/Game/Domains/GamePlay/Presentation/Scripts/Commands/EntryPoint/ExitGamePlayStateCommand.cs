using Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayStateCommand : BaseCommand, ICommandVoid
    {
        private IGameInputActionsController _gameInputActionsController;
        private IClientNetworkManager _clientNetworkManager;
        private IJoinResponsePacketHandler _joinResponsePacketHandler;
        private IInputDeviceChangedListenerService _inputDeviceChangedListenerService;
        private IChooseNetworkRoleUIController _chooseNetworkRoleUIController;

        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _joinResponsePacketHandler = _diContainer.Resolve<IJoinResponsePacketHandler>();
            _inputDeviceChangedListenerService = _diContainer.Resolve<IInputDeviceChangedListenerService>();
            _chooseNetworkRoleUIController = _diContainer.Resolve<IChooseNetworkRoleUIController>();
        }

        public void Execute()
        {
            _clientNetworkManager.InitExitPoint();
            _gameInputActionsController.DisableInputs();
            _joinResponsePacketHandler.InitExitPoint();
            _inputDeviceChangedListenerService.InitExitPoint();
            _chooseNetworkRoleUIController.InitEntryPoint();
        }
    }
}