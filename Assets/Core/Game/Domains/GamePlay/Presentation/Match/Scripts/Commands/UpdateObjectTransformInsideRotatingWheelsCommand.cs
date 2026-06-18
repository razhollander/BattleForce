using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.LavaWalls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateObjectTransformInsideRotatingWheelsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IEnvironmentLavaWallsControllers _environmentLavaWallsControllers;
        private IMatchEnvironmentWallsControllers _environmentWallsControllers;
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private IEnvironmentSpikeControllers _environmentSpikeControllers;
        private IEnvironmentTeleportGateControllers _environmentTeleportGateControllers;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _environmentLavaWallsControllers = _diContainer.Resolve<IEnvironmentLavaWallsControllers>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchEnvironmentWallsControllers>();
            _environmentSpringControllers = _diContainer.Resolve<IEnvironmentSpringControllers>();
            _environmentSpikeControllers = _diContainer.Resolve<Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc.IEnvironmentSpikeControllers>();
            _environmentTeleportGateControllers = _diContainer.Resolve<IEnvironmentTeleportGateControllers>();
        }

        public void Execute()
        {
            foreach (var rotatingWheelModel in _matchDataService.RotatingWheels)
            {
                foreach (var lavaWallId in rotatingWheelModel.LavaWallIds)
                {
                    _environmentLavaWallsControllers.UpdateLavaWallTransform(lavaWallId);
                }

                foreach (var wallId in rotatingWheelModel.WallIds)
                {
                    _environmentWallsControllers.UpdateWallTransform(wallId);
                }

                foreach (var springId in rotatingWheelModel.SpringIds)
                {
                    _environmentSpringControllers.UpdateSpringTransform(springId);
                }

                foreach (var spikeId in rotatingWheelModel.SpikeIds)
                {
                    _environmentSpikeControllers.UpdateSpikeTransform(spikeId);
                }

                foreach (var pairId in rotatingWheelModel.TeleportGates)
                {
                    _environmentTeleportGateControllers.UpdateTeleportGateTransform(pairId.BelongToPairId, pairId.IsGateA);
                }
            }
        }
    }
}