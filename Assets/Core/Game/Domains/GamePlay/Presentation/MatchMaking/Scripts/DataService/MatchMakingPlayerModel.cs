using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchMakingData
{
    public class MatchMakingPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public MatchMakingPlayerSpaceshipStateS2C Spaceship;

        public MatchMakingPlayerModel(ushort playerId, string playerName, MatchMakingPlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Spaceship = spaceship;
        }
    }
}