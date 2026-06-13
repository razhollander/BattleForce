namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersTalentsCooldowns
{
    public interface IPlayerTalentsCooldownsService
    {
        void AddPlayer(ushort playerId);

        void AddCooldownMultiplierForPlayer(int currentTick, ushort playerId, TalentCooldownMultiplierType talentCooldownMultiplierType, float multiplier,
            bool shouldSendNetEvent = true);

        void RemoveCooldownMultiplierForPlayer(int currentTick, ushort playerId, TalentCooldownMultiplierType talentCooldownMultiplierType, bool shouldSendNetEvent = true);
        float GetPlayerTotalTalentCooldownMultiplier(ushort playerId);
    }
}