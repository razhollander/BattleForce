using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.PlayerTeleportEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GainBoltEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.PowerUps.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.EntryPoint
{
    public class StartGamePlayMatchCommand: BaseCommand, ICommandVoid
    {
        private GamePlayMatchInitiatorEnterData _enterData;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IPowerUpBallControllers _powerUpBallControllers;
        private IPowerUpBallObtainedEffectController _powerUpBallObtainedEffectController;
        private ITalentCardControllers _talentCardControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchBulletControllers _bulletControllers;
        private ITickProcessor _tickProcessor;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IMatchDataService _matchDataService;
        private ICommandFactory _commandFactory;
        private IClientMatchPresentationTickProcessor _clientMatchPresentationTickProcessor;
        private IStartStagePacketHandler _startStagePacketHandler;
        private IGainBoltEffectController _gainBoltEffectController;
        private IPlayerTeleportEffectController _playerTeleportEffectController;
        private IEnvironmentTeleportGateControllers _environmentTeleportGateControllers;
        private IEnvironmentFieldBarrierControllers _environmentFieldBarrierControllers;
        private ISwapFieldControllers _swapFieldControllers;
        private IKOProjectilesControllers _koProjectilesControllers;
        private IGrapplingHookProjectilesControllers _grapplingHookProjectilesControllers;
        private IDashPulseGustEffectController _dashPulseGustEffectController;
        private Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts.IMagneticPullEffectController _magneticPullEffectController;

        public StartGamePlayMatchCommand SetEnterData(GamePlayMatchInitiatorEnterData enterEnterData)
        {
            _enterData = enterEnterData;
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
            _environmentSpringControllers = _diContainer.Resolve<IEnvironmentSpringControllers>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _clientMatchPresentationTickProcessor = _diContainer.Resolve<IClientMatchPresentationTickProcessor>();
            _startStagePacketHandler = _diContainer.Resolve<IStartStagePacketHandler>();
            _gainBoltEffectController = _diContainer.Resolve<IGainBoltEffectController>();
            _playerTeleportEffectController = _diContainer.Resolve<IPlayerTeleportEffectController>();
            _environmentTeleportGateControllers = _diContainer.Resolve<IEnvironmentTeleportGateControllers>();
            _environmentFieldBarrierControllers = _diContainer.Resolve<IEnvironmentFieldBarrierControllers>();
            _swapFieldControllers = _diContainer.Resolve<ISwapFieldControllers>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _grapplingHookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _dashPulseGustEffectController = _diContainer.Resolve<IDashPulseGustEffectController>();
            _magneticPullEffectController = _diContainer.Resolve<Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts.IMagneticPullEffectController>();
        }

        public void Execute()
        {
            _fullTickPacketsHandler.InitEntryPoint();
            _startStagePacketHandler.InitEntryPoint();
            _talentCardControllers.InitEntryPoint();
            _environmentLavaWallsControllers.InitEntryPoint();
            _environmentFieldBarrierControllers.InitEntryPoint();
            _talentCardObtainedEffectController.InitEntryPoint();
            _powerUpBallControllers.InitEntryPoint();
            _swapFieldControllers.InitEntryPoint();
            _koProjectilesControllers.InitEntryPoint();
            _grapplingHookProjectilesControllers.InitEntryPoint();
            _powerUpBallObtainedEffectController.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            _environmentSpringControllers.InitEntryPoint();
            _playerTeleportEffectController.InitEntryPoint();
            _environmentTeleportGateControllers.InitEntryPoint();
            _commandFactory.CreateCommandVoid<SyncMatchSimulationStateCommand>()
                 .SetSimulationState(_enterData.InitialState)
                 .Execute();
             _matchDataService.SetLocalPlayer(_enterData.LocalPlayerId);
            _gainBoltEffectController.InitEntryPoint();
            _dashPulseGustEffectController.InitEntryPoint();
            _magneticPullEffectController.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _clientMatchPresentationTickProcessor.InitEntryPoint();
        }
    }
}
