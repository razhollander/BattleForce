using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public ushort TeamId;
        public PlayerSpaceshipStateS2C Spaceship;
        public int SpinEndOnTick;
        
        public MatchPlayerModel(ushort playerId, string playerName, ushort teamId, PlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            TeamId = teamId;
            Spaceship = spaceship;
        }
    }
}