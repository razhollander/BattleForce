using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator
{
    public class GamePlayMatchInitiatorEnterData : IInitiatorEnterData
    {
        public readonly MatchSimulationStateS2C InitialState;
        public readonly int StateOccouredOnTick;
        public readonly ushort LocalPlayerId;

        public GamePlayMatchInitiatorEnterData(MatchSimulationStateS2C initialState, ushort localPlayerId, int stateOccouredOnTick)
        {
            InitialState = initialState;
            LocalPlayerId = localPlayerId;
            StateOccouredOnTick = stateOccouredOnTick;
        }
    }
}
