using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents
{
    public interface IOverrideableNetEventsService
    {
        void RegisterAllOverridableNetEvents();
        void OverrideUpdateTalentStockEvent(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick);
        void OverridePlayerMaxShootCooldownChangedEvent(int onTick, ushort playerId, float maxShootCooldown);
    }
}