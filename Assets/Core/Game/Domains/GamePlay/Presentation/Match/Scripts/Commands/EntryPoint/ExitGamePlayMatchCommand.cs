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
        private IMatchStartMatchPacketHandler _startMatchPacketHandler;

        public override void ResolveDependencies()
        {
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientMatchPresentationTickProcessor>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _startMatchPacketHandler = _diContainer.Resolve<IMatchStartMatchPacketHandler>();
        }

        public void Execute()
        {
            _clientPresentationTickProcessor.InitExitPoint();
            _tickProcessor.StopTick();
            _fullTickPacketsHandler.InitExitPoint();
            _startMatchPacketHandler.InitExitPoint();
        }
    }
}
