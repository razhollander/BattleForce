using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayMatchMakingCommand: BaseCommand, ICommandVoid
    {
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IClientMatchMakingPresentationTickProcessor _clientMatchMakingPresentationTickProcessor;

        public override void ResolveDependencies()
        {
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _clientMatchMakingPresentationTickProcessor = _diContainer.Resolve<IClientMatchMakingPresentationTickProcessor>();
        }

        public void Execute()
        {
            _fullTickPacketsHandler.InitExitPoint();
            _clientMatchMakingPresentationTickProcessor.StopTick();
        }
    }
}
