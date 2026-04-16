using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService
{
    public class MatchMakingPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public ushort TeamId;
        public MatchMakingPlayerSpaceshipStateS2C Spaceship;

        public MatchMakingPlayerModel(ushort playerId, string playerName, MatchMakingPlayerSpaceshipStateS2C spaceship, ushort teamId)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Spaceship = spaceship.GetClone();
            TeamId = teamId;
        }
    }
}