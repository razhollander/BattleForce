using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public ushort TeamId;
        public int StageScore;
        public PlayerSpaceshipStateS2C Spaceship;

        public MatchPlayerModel(ushort playerId, string playerName, ushort teamId, int stageScore, PlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            TeamId = teamId;
            StageScore = stageScore;
            Spaceship = spaceship.GetClone();
        }
    }
}