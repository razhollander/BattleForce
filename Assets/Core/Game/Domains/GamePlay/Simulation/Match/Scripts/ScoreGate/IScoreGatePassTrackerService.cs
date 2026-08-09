using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate
{
    // Tracks the per-tick data GatePass pass-detection needs: each player's previous position (for the segment-crossing
    // test) and a per player-gate cooldown so a player wedged in the gap cannot farm points on physics jitter.
    public interface IScoreGatePassTrackerService
    {
        void ClearAllData();

        // Drops the stored previous position so the next tick seeds a fresh one and no crossing is tested for this
        // player this tick. MUST be called by every talent/gate path that teleports a player (Swap, teleport gate,
        // Soul respawn), otherwise a jump that straddles the gate line awards a free point.
        void InvalidatePreviousPosition(ushort playerId);

        bool TryGetPreviousPosition(ushort playerId, out Vector2 previousPosition);
        void SetPreviousPosition(ushort playerId, Vector2 position);

        bool IsPassScoreOnCooldown(ushort playerId, ushort scoreGateId, int currentTick);
        void StartPassScoreCooldown(ushort playerId, ushort scoreGateId, int cooldownEndTick);
    }
}
