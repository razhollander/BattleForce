using System.Threading;
using CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Mvc.GameInputActions;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace CoreDomain.GameDomain.GameStateDomain.GamePlayDomain.Scripts.Commands.EntryPoint
{
    public class StartGamePlayStateCommand : BaseCommand, ICommandAsync
    {
        private ICommandFactory _commandFactory;
        private IWorldCameraController _worldCameraController;
        private IAudioService _audioService;
        private IGameInputActionsController _gameInputActionsController;

        private GamePlayInitiatorEnterData _enterData; // kept this for future use

        public StartGamePlayStateCommand SetEnterData(GamePlayInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            //_audioService.PlayAudio(AudioClipType.GamePlayBGMusic, AudioChannelType.Master, AudioPlayType.Loop);
            _gameInputActionsController.EnableInputs();
        }
    }
}