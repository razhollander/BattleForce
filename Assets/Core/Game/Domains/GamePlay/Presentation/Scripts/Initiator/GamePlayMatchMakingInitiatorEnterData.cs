using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator
{
    public class GamePlayMatchMakingInitiatorEnterData : IInitiatorEnterData
    {
        public readonly string IPAddress;
        public readonly int Port;
        public readonly bool IsHost;
        public readonly int PlayerId;
        public MatchMakingSimulationStateS2C SimulationState;
        public int StateOccuredOnTick;
        
        public GamePlayMatchMakingInitiatorEnterData(MatchMakingSimulationStateS2C simulationState,string ipAddress, int port, bool isHost, int stateOccuredOnTick, int playerId)
        {
            SimulationState = simulationState;
            IPAddress = ipAddress;
            Port = port;
            IsHost = isHost;
            PlayerId = playerId;
            StateOccuredOnTick = stateOccuredOnTick;
        }
    }
}
