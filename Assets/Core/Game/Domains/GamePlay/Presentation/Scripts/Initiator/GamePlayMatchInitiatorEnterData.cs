using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.CoreInitiator.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator
{
    public class GamePlayMatchInitiatorEnterData : IInitiatorEnterData
    {
        public MatchSimulationStateS2C InitialState { get; set; }
        public ushort LocalPlayerId { get; set; }

        public GamePlayMatchInitiatorEnterData()
        {
        }
    }
}
