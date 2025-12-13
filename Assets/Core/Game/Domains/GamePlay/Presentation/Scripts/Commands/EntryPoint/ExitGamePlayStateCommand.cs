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
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IClientPresentationTickProcessor _clientPresentationTickProcessor;
        private ITickProcessor _tickProcessor;

        public override void ResolveDependencies()
        {
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientPresentationTickProcessor>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
        }

        public void Execute()
        {
            _clientNetworkManager.InitExitPoint();
            //_audioService.RemoveAudioClips(_gamePlayAudioClipsScriptableObject);
            //_commandFactory.CreateCommandVoid<DisposeLevelCommand>().SetShouldReleaseAssetsFromMemory(true).Execute();
            _gameInputActionsController.DisableInputs();
            _fullTickPacketsHandler.InitExitPoint();
            _clientPresentationTickProcessor.StopTick();
            _tickProcessor.StopTick();
            //_gamePlayUiController.InitExitPoint();
        }
    }
}