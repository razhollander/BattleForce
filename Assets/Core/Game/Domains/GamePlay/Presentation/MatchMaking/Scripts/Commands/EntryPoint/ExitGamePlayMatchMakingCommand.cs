using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayMatchMakingCommand: BaseCommand, ICommandVoid
    {
        private IFullTickPacketsHandler _fullTickPacketsHandler;

        public override void ResolveDependencies()
        {
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
        }

        public void Execute()
        {
            _fullTickPacketsHandler.InitExitPoint();
        }
    }
}
