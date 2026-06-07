using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator
{
    public class GamePlayMatchMakingInitiatorEnterData : IInitiatorEnterData
    {
        public readonly string IPAddress;
        public readonly int Port;
        public readonly bool IsHost;
        public MatchMakingSimulationStateS2C SimulationState;
        public int StateOccuredOnTick;
        public Dictionary<ushort, int> PlayerIdToDeviceIdDictionary;

        public GamePlayMatchMakingInitiatorEnterData(MatchMakingSimulationStateS2C simulationState,string ipAddress, int port, bool isHost, int stateOccuredOnTick, Dictionary<ushort, int> playerIdToDeviceIdDictionary)
        {
            SimulationState = simulationState;
            IPAddress = ipAddress;
            Port = port;
            IsHost = isHost;
            StateOccuredOnTick = stateOccuredOnTick;
            PlayerIdToDeviceIdDictionary = new Dictionary<ushort, int>(playerIdToDeviceIdDictionary.Count);

            foreach (var kvp in playerIdToDeviceIdDictionary)
            {
                PlayerIdToDeviceIdDictionary.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
