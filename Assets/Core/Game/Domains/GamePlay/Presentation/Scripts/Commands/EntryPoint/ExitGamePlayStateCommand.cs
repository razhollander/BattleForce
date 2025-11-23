using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayStateCommand : BaseCommand, ICommandVoid
    {
        private IGameInputActionsController _gameInputActionsController;
        private ICommandFactory _commandFactory;
        private IAudioService _audioService;
        private IClientNetworkManager _clientNetworkManager;
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;

        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
        }

        public void Execute()
        {
            _clientNetworkManager.InitExitPoint();
            //_audioService.RemoveAudioClips(_gamePlayAudioClipsScriptableObject);
            //_commandFactory.CreateCommandVoid<DisposeLevelCommand>().SetShouldReleaseAssetsFromMemory(true).Execute();
            _gameInputActionsController.DisableInputs();
            _playerJoinPacketsHandler.InitExitPoint();
            //_gamePlayUiController.InitExitPoint();
        }
    }
}