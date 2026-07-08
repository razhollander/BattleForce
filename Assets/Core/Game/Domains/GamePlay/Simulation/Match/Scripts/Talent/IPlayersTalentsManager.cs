using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public interface IPlayersTalentsManager
    {
        void AddPlayer(ushort playerId);
        void RemovePlayer(ushort playerId);
        bool TryAddTalentToPlayer(TalentType talentType, ushort playerId, int tick, out TalentStateS2C newTalent, out bool didReplaceExistingTalent);
        bool TrySwitchToNextTalent(ushort playerId);
        bool TrySwitchToTalent(ushort playerId, int talentIndex);
        void ProcessPlayerTalentInput(ushort playerId, TalentType talentType, int tick, bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, float deltaTime);
        void ProcessAllTalentsTickOfPlayer(ushort playerId, int tick, float deltaTime);
        void CompleteSwapTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick);
        void ResetAllTalentsData();
        void HitKOTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick);
        void HitKOTalentWithWall(ushort casterId);
        void HitGrapplingHookWithWall(ushort casterId, ushort projectileId, ushort wallId, int tick);
        void CatchFishingRodWithEnemy(ushort casterId, ushort enemyPlayerId, int tick);
        void HitFishingRodWithWall(ushort casterId, ushort projectileId, int tick);
        void StopTalentIfActive(TalentType talentType, ushort playerId, int tick);
        void TryHeadbuttHitEnemy(ushort potentialCasterId, ushort potentialEnemyId, int tick);
    }
}