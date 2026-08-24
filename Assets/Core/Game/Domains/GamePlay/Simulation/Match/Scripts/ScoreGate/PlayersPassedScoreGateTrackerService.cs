using System.Numerics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate
{
    public class PlayersPassedScoreGateTrackerService : IPlayersPassedScoreGateTrackerService
    {
        private const int PLAYER_ID_KEY_SHIFT = 16; // packs (playerId, scoreGateId) into one int cooldown key

        private readonly CapacityDict<ushort, Vector2> _previousPositionPerPlayerId;
        private readonly CapacityDict<int, int> _cooldownEndTickPerPlayerGate;

        public PlayersPassedScoreGateTrackerService(NetworkConfig networkConfig)
        {
            var maxPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            var maxScoreGates = networkConfig.MaxCap.ConcurrentScoreGates;
            _previousPositionPerPlayerId = new CapacityDict<ushort, Vector2>(maxPlayers);
            _cooldownEndTickPerPlayerGate = new CapacityDict<int, int>(maxPlayers * maxScoreGates);
        }

        public void ClearAllData()
        {
            _previousPositionPerPlayerId.Clear();
            _cooldownEndTickPerPlayerGate.Clear();
        }

        public void InvalidatePreviousPosition(ushort playerId)
        {
            _previousPositionPerPlayerId.Remove(playerId);
        }

        public bool TryGetPlayerPreviousPosition(ushort playerId, out Vector2 previousPosition)
        {
            return _previousPositionPerPlayerId.TryGetValue(playerId, out previousPosition);
        }

        public void SetPlayerPreviousPosition(ushort playerId, Vector2 position)
        {
            _previousPositionPerPlayerId[playerId] = position;
        }

        public bool IsPlayerPassScoreOnCooldown(ushort playerId, ushort scoreGateId, int currentTick)
        {
            var key = BuildPlayIdAndScoreGateIdKey(playerId, scoreGateId);
            return _cooldownEndTickPerPlayerGate.TryGetValue(key, out var cooldownEndTick) && currentTick < cooldownEndTick;
        }

        public void StartPlayerPassScoreCooldown(ushort playerId, ushort scoreGateId, int cooldownEndTick)
        {
            _cooldownEndTickPerPlayerGate[BuildPlayIdAndScoreGateIdKey(playerId, scoreGateId)] = cooldownEndTick;
        }

        private static int BuildPlayIdAndScoreGateIdKey(ushort playerId, ushort scoreGateId)
        {
            return (playerId << PLAYER_ID_KEY_SHIFT) | scoreGateId;
        }
    }
}
