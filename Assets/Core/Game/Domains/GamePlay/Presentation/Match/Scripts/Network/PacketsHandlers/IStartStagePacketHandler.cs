using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Network.PacketsHandlers
{
    public interface IStartStagePacketHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
    }
}