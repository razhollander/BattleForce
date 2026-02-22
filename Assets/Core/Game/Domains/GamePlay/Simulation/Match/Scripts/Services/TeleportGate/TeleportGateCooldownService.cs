using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Utils.CustomCollections;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate
{
    public class TeleportGateCooldownService : ITeleportGateService
    {
        private readonly CapacityDict<ushort, int> _lastTeleportTickPerPlayer;
        private readonly SimulationGamePlayConfig _config;
        private readonly NetworkConfig _networkConfig;

        public TeleportGateCooldownService(SimulationGamePlayConfig config, NetworkConfig networkConfig)
        {
            _config = config;
            _networkConfig = networkConfig;
            _lastTeleportTickPerPlayer = new CapacityDict<ushort, int>(_networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void RegisterTeleport(ushort playerId, int currentTick)
        {
            if (_lastTeleportTickPerPlayer.ContainsKey(playerId))
            {
                _lastTeleportTickPerPlayer[playerId] = currentTick;
            }
            else
            {
                _lastTeleportTickPerPlayer.Add(playerId, currentTick);
            }
        }

        public bool IsTeleportOnCooldown(ushort playerId, int currentTick)
        {
            if (_lastTeleportTickPerPlayer.TryGetValue(playerId, out var lastTick))
            {
                // Calculate cooldown in ticks
                int cooldownTicks = (int)(_config.TeleportGateCooldown * _networkConfig.TicksPerSeconds);
                return (currentTick - lastTick) < cooldownTicks;
            }
            return false;
        }

        public void ClearData()
        {
            _lastTeleportTickPerPlayer.Clear();
        }
    }
}
