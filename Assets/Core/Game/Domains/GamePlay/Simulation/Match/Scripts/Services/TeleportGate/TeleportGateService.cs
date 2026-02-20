using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Utils.CustomCollections;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate
{
    public class TeleportGateService : ITeleportGateService
    {
        private readonly CapacityDict<ushort, int> _lastTeleportTickPerPlayer;
        private readonly SimulationGamePlayConfig _config;
        private readonly NetworkConfig _networkConfig;

        public TeleportGateService(SimulationGamePlayConfig config, NetworkConfig networkConfig)
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

        public bool CanTeleport(ushort playerId, int currentTick)
        {
            if (_lastTeleportTickPerPlayer.TryGetValue(playerId, out var lastTick))
            {
                // Calculate cooldown in ticks
                int cooldownTicks = (int)(_config.TeleportGateCooldown * _networkConfig.TicksPerSeconds);
                return (currentTick - lastTick) >= cooldownTicks;
            }
            return true;
        }

        public void ClearData()
        {
            _lastTeleportTickPerPlayer.Clear();
        }
    }
}
