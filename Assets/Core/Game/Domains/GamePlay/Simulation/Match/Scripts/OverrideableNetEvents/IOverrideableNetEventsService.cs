using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents
{
    public interface IOverrideableNetEventsService
    {
        void RegisterAllOverridableNetEvents();
        void OverrideUpdateTalentStockEvent(int onTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick);
        void OverridePlayerMaxShootCooldownChangedEvent(int onTick, ushort playerId, float maxShootCooldown, float cooldownSecondsLeft);
        void OverridePlayerTalentsMaxCooldownMultiplierChangedEvent(int onTick, ushort playerId, float allTalentsCooldownMultiplier, FixedOrderedList<TalentStateS2C> playerTalents);
    }
}