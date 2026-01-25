using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.EntryPoint
{
    public class StartGamePlayMatchCommand: BaseCommand, ICommandAsync
    {
        private GamePlayMatchInitiatorEnterData _enterEnterData;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private ITalentCardControllers _talentCardControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchBulletControllers _bulletControllers;
        private ITickProcessor _tickProcessor;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;

        public StartGamePlayMatchCommand SetEnterData(GamePlayMatchInitiatorEnterData enterEnterData)
        {
            _enterEnterData = enterEnterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _powerUpBallControllers = _diContainer.Resolve<IPowerUpBallControllers>();
            _powerUpBallObtainedEffectController = _diContainer.Resolve<IPowerUpBallObtainedEffectController>();
            _talentCardControllers = _diContainer.Resolve<ITalentCardControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchBulletControllers>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchEnvironmentWallsControllers>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _talentCardControllers.InitEntryPoint();
            _environmentLavaWallsControllers.InitEntryPoint();
            _talentCardObtainedEffectController.InitEntryPoint();
            _powerUpBallControllers.InitEntryPoint();
            _powerUpBallObtainedEffectController.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            
        }
    }
}
