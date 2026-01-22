using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.UI;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint
{
    public class StartGamePlayStateCommand : BaseCommand, ICommandAsync
    {
        private ICommandFactory _commandFactory;
        private IAudioService _audioService;
        private IGameInputActionsController _gameInputActionsController;
        private IChooseNetworkRoleUIController _chooseNetworkRoleUIController;
        private IPlayerControllers _playerControllers;
        private ITickProcessor _tickProcessor;
        private IBulletControllers _bulletControllers;
        private IEnvironmentWallsControllers _environmentWallsControllers;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private ITalentCardControllers _talentCardControllers;

        private GamePlayInitiatorEnterData _enterData; // kept this for future use

        public StartGamePlayStateCommand SetEnterData(GamePlayInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
            _chooseNetworkRoleUIController = _diContainer.Resolve<IChooseNetworkRoleUIController>();
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IEnvironmentWallsControllers>();
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _powerUpBallObtainedEffectController = _diContainer.Resolve<IPowerUpBallObtainedEffectController>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            //_audioService.PlayAudio(AudioClipType.GamePlayBGMusic, AudioChannelType.Master, AudioPlayType.Loop);
            _gameInputActionsController.EnableInputs();
            _chooseNetworkRoleUIController.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _talentCardControllers.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            _environmentLavaWallsControllers.InitEntryPoint();
            _talentCardObtainedEffectController.InitEntryPoint();
            _powerUpBallControllers.InitEntryPoint();
            _powerUpBallObtainedEffectController.InitEntryPoint();
        }
    }
}