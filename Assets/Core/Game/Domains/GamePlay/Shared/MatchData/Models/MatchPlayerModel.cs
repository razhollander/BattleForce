namespace Core.Game.Domains.GamePlay.Shared
{
    public class MatchPlayerModel
    {
        public int PlayerId;
        public string PlayerName;

        public MatchPlayerModel(int playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}