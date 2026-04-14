using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public interface IPlayersTalentsManager
    {
        void AddPlayer(ushort playerId);
        void RemovePlayer(ushort playerId);
        bool TryAddTalentToPlayer(TalentType talentType, ushort playerId, int tick, out TalentStateS2C newTalent, out bool didReplaceExistingTalent);
        bool TrySwitchToNextTalent(ushort playerId);
        void ProcessPlayerTalentInput(ushort playerId, TalentType talentType, int tick, bool isTalentInputPressed, float deltaTime);
        void ProcessAllTalentsTickOfPlayer(ushort playerId, int tick, float deltaTime);
        void CompleteSwapTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick);
        void ResetAllTalentsData();
        void HitKOTalentWithEnemy(ushort casterId, ushort enemyPlayerId, int tick);
        void HitKOTalentWithWall(ushort casterId);
        void HitGrapplingHookWithWall(ushort casterId, ushort projectileId, ushort wallId, int tick);
    }
}