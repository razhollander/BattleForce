using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Shared
{
    public class MatchPlayerModel
    {
        public ushort PlayerId;
        public string PlayerName;
        public PlayerSpaceshipStateS2C Spaceship;

        public MatchPlayerModel(ushort playerId, string playerName, PlayerSpaceshipStateS2C spaceship)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Spaceship = spaceship;
        }
    }
}