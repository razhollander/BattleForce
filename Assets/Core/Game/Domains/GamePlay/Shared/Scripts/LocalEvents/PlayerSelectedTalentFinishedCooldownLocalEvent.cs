namespace Core.Game.Domains.GamePlay.Shared.Scripts.LocalEvents
{
    public class PlayerSelectedTalentFinishedCooldownLocalEvent
    {
        public ushort PlayerId;

        public PlayerSelectedTalentFinishedCooldownLocalEvent(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}