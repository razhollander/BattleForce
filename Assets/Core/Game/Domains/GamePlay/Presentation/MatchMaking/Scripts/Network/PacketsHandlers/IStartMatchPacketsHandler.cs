using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public interface IStartMatchPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
    }
}
