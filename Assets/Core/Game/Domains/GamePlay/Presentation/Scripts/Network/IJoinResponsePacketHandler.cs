using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public interface IJoinResponsePacketHandler
    {
        bool DidReceiveJoinResponse { get; }
        JoinResponsePacketS2C JoinResponse { get; }
        void InitEntryPoint();
        void InitExitPoint();
        void Reset();
    }
}