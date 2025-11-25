using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Shared
{
    public class MatchPlayerModel
    {
        public int PlayerId;
        public string PlayerName;
        public PlayerTransformStateS2C TransformState;

        public MatchPlayerModel(int playerId, string playerName, PlayerTransformStateS2C transformStateS2C)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            TransformState = transformStateS2C;
        }
    }
}