namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface IMatchPlayerTimersService
    {
        void StartPlayerTalentTimer(ushort playerId, int talentIndex, int initialServerTick);
        float GetPlayerTalentTimer(ushort playerId, int talentIndex, int currentServerTick);
    }
}
