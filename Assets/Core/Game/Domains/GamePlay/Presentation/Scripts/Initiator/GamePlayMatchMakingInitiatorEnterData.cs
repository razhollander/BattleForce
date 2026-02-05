using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator
{
    public class GamePlayMatchMakingInitiatorEnterData : IInitiatorEnterData
    {
        public readonly string IPAddress;
        public readonly int Port;
        public readonly bool IsHost;

        public GamePlayMatchMakingInitiatorEnterData(string ipAddress, int port, bool isHost)
        {
            IPAddress = ipAddress;
            Port = port;
            IsHost = isHost;
        }
    }
}
