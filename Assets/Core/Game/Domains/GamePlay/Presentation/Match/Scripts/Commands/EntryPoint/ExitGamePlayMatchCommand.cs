using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.TalentCards.Scripts.ObtainedEffect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayMatchCommand : BaseCommand, ICommandVoid
    {
        private IClientMatchPresentationTickProcessor _clientPresentationTickProcessor;
        private ITickProcessor _tickProcessor;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IStartStagePacketHandler _startStagePacketHandler;
        private IBackgroundParallaxController _backgroundParallaxController;
        private ITalentCardObtainedEffectController _talentCardObtainedEffectController;
        private IGalacticPullStarEffectControllers _galacticPullStarEffectControllers;
        private IFrigidBlocksControllers _frigidBlocksControllers;

        public override void ResolveDependencies()
        {
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientMatchPresentationTickProcessor>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _startStagePacketHandler = _diContainer.Resolve<IStartStagePacketHandler>();
            _backgroundParallaxController = _diContainer.Resolve<IBackgroundParallaxController>();
            _talentCardObtainedEffectController = _diContainer.Resolve<ITalentCardObtainedEffectController>();
            _galacticPullStarEffectControllers = _diContainer.Resolve<IGalacticPullStarEffectControllers>();
            _frigidBlocksControllers = _diContainer.Resolve<IFrigidBlocksControllers>();
        }

        public void Execute()
        {
            _backgroundParallaxController.InitExitPoint();
            _clientPresentationTickProcessor.InitExitPoint();
            _tickProcessor.StopTick();
            _fullTickPacketsHandler.InitExitPoint();
            _startStagePacketHandler.InitExitPoint();
            _talentCardObtainedEffectController.InitExitPoint();
            _galacticPullStarEffectControllers.InitExitPoint();
            _frigidBlocksControllers.InitExitPoint();
        }
    }
}
