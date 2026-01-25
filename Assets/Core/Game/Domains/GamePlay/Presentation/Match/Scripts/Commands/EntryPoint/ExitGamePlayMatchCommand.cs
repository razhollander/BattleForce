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

        public override void ResolveDependencies()
        {
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientMatchPresentationTickProcessor>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
        }

        public void Execute()
        {
            _clientPresentationTickProcessor.StopTick();
            _tickProcessor.StopTick();
            _fullTickPacketsHandler.InitExitPoint();
        }
    }
}
