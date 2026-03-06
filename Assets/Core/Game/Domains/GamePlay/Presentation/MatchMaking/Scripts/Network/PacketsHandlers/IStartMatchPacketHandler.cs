using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public interface IStartMatchPacketHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessStartMatchPacket();
    }
}