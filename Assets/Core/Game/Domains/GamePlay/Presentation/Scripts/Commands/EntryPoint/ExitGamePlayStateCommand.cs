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

        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _joinResponsePacketHandler = _diContainer.Resolve<IJoinResponsePacketHandler>();
        }

        public void Execute()
        {
            _clientNetworkManager.InitExitPoint();
            _gameInputActionsController.DisableInputs();
            _joinResponsePacketHandler.InitExitPoint();
        }
    }
}