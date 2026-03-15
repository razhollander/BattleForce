namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public class MatchPlayerTimers
    {
        public ushort PlayerId;
        public string[] TalentTimers;

        public MatchPlayerTimers(ushort playerId, int maxTalents)
        {
            PlayerId = playerId;
            TalentTimers = new string[maxTalents];
        }
    }
}
