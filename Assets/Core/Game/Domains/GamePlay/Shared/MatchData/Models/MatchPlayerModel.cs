using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Shared
{
    public class MatchPlayerModel
    {
        public int PlayerId;
        public string PlayerName;
        public PlayerSpaceshipStateS2C Spaceship;

        public MatchPlayerModel(int playerId, string playerName, PlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Spaceship = spaceship;
        }
    }
}