namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface IMatchPlayerTimersService
    {
        void StartPlayerTalentTimer(ushort playerId, int talentIndex, int initialTick);
        float GetPlayerTalentTimer(ushort playerId, int talentIndex);
    }
}
