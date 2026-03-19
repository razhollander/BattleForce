namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Timer
{
    public interface IMatchPlayerTimersService
    {
        void StartPlayerTalentTimer(ushort playerId, int talentIndex, int endServerTick);
        float GetPlayerTalentTimerSecondsLeft(ushort playerId, int talentIndex, int currentServerTick);
    }
}
