using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayMatchMakingCommand: BaseCommand, ICommandVoid
    {
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IClientMatchMakingPresentationTickProcessor _clientMatchMakingPresentationTickProcessor;
        private IStartMatchPacketsHandler _startMatchPacketsHandler;

        public override void ResolveDependencies()
        {
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _clientMatchMakingPresentationTickProcessor = _diContainer.Resolve<IClientMatchMakingPresentationTickProcessor>();
            _startMatchPacketsHandler = _diContainer.Resolve<IStartMatchPacketsHandler>();
        }

        public void Execute()
        {
            _startMatchPacketsHandler.InitExitPoint();
            _fullTickPacketsHandler.InitExitPoint();
            _clientMatchMakingPresentationTickProcessor.StopTick();
        }
    }
}
