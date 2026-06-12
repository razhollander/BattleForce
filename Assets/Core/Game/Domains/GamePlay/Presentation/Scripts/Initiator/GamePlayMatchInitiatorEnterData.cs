using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator
{
    public class GamePlayMatchInitiatorEnterData : IInitiatorEnterData
    {
        public readonly MatchSimulationStateS2C InitialState;
        public readonly int StateOccouredOnTick;
        public Dictionary<ushort, int> PlayerIdToDeviceIdDictionary;
        
        public GamePlayMatchInitiatorEnterData(MatchSimulationStateS2C initialState, int stateOccouredOnTick, Dictionary<ushort,int>playerIdToDeviceIdDictionary)
        {
            InitialState = initialState;
            StateOccouredOnTick = stateOccouredOnTick;
            PlayerIdToDeviceIdDictionary = new Dictionary<ushort, int>(playerIdToDeviceIdDictionary.Count);

            foreach (var kvp in playerIdToDeviceIdDictionary)
            {
                PlayerIdToDeviceIdDictionary.Add(kvp.Key, kvp.Value);
            }
        }
    }
}

