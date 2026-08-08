using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public ushort TeamId;
        public int MolesHitScore; // this player's contribution to his team's WhacAMole score
        public PlayerSpaceshipStateS2C Spaceship;

        public MatchPlayerModel(ushort playerId, string playerName, ushort teamId, int molesHitScore, PlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            TeamId = teamId;
            MolesHitScore = molesHitScore;
            Spaceship = spaceship.GetClone();
        }
    }
}