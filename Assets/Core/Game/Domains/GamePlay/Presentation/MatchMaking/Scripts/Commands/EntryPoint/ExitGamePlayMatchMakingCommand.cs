using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class ExitGamePlayMatchMakingCommand: BaseCommand, ICommandVoid
    {
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IClientMatchMakingPresentationTickProcessor _clientMatchMakingPresentationTickProcessor;
        private IStartMatchPacketHandler _startMatchPacketHandler;

        public override void ResolveDependencies()
        {
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _clientMatchMakingPresentationTickProcessor = _diContainer.Resolve<IClientMatchMakingPresentationTickProcessor>();
            _startMatchPacketHandler = _diContainer.Resolve<IStartMatchPacketHandler>();
        }

        public void Execute()
        {
            _startMatchPacketHandler.InitExitPoint();
            _fullTickPacketsHandler.InitExitPoint();
            LogService.LogError("Exit match making!");
            _clientMatchMakingPresentationTickProcessor.StopTick();
        }
    }
}
