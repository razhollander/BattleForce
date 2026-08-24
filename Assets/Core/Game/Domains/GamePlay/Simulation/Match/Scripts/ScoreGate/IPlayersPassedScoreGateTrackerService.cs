using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate
{
    public interface IPlayersPassedScoreGateTrackerService
    {
        void ClearAllData();

        // Drops the stored previous position so the next tick seeds a fresh one and no crossing is tested for this
        // player this tick. MUST be called by every talent/gate path that teleports a player (Swap, teleport gate,
        // Soul respawn), otherwise a jump that straddles the gate line awards a free point.
        void InvalidatePreviousPosition(ushort playerId);
        bool TryGetPlayerPreviousPosition(ushort playerId, out Vector2 previousPosition);
        void SetPlayerPreviousPosition(ushort playerId, Vector2 position);
        bool IsPlayerPassScoreOnCooldown(ushort playerId, ushort scoreGateId, int currentTick);
        void StartPlayerPassScoreCooldown(ushort playerId, ushort scoreGateId, int cooldownEndTick);
    }
}
