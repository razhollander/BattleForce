using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayStateCommand : BaseCommand, ICommandVoid
    {
        private IGameInputActionsController _gameInputActionsController;
        private ICommandFactory _commandFactory;
        private IAudioService _audioService;
        private IClientNetworkManager _clientNetworkManager;
        private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
        private ISimulationStatePacketsHandler _simulationStatePacketsHandler;
        private IClientPresentationTickProcessor _clientPresentationTickProcessor;
        private ITickProcessor _tickProcessor;

        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
            _simulationStatePacketsHandler = _diContainer.Resolve<ISimulationStatePacketsHandler>();
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientPresentationTickProcessor>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
        }

        public void Execute()
        {
            _clientNetworkManager.InitExitPoint();
            //_audioService.RemoveAudioClips(_gamePlayAudioClipsScriptableObject);
            //_commandFactory.CreateCommandVoid<DisposeLevelCommand>().SetShouldReleaseAssetsFromMemory(true).Execute();
            _gameInputActionsController.DisableInputs();
            _playerJoinPacketsHandler.InitExitPoint();
            _simulationStatePacketsHandler.InitExitPoint();
            _clientPresentationTickProcessor.StopTick();
            _tickProcessor.StopTick();
            //_gamePlayUiController.InitExitPoint();
        }
    }
}