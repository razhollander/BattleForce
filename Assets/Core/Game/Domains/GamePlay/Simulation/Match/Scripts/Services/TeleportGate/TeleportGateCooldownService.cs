using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Utils.CustomCollections;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate
{
    public class TeleportGateCooldownService : ITeleportGateService
    {
        private readonly CapacityDict<ushort, int> _lastTeleportTickPerPlayer;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        public TeleportGateCooldownService(ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _lastTeleportTickPerPlayer = new CapacityDict<ushort, int>(_networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void RegisterTeleport(ushort playerId, int currentTick)
        {
            _lastTeleportTickPerPlayer[playerId] = currentTick;
        }

        public bool IsTeleportOnCooldown(ushort playerId, int currentTick)
        {
            if (_lastTeleportTickPerPlayer.TryGetValue(playerId, out var lastTick))
            {
                var cooldownTicks = (int)(_gamePlayConfigService.GamePlayConfig.TeleportGateCooldownInSeconds * _networkConfig.TicksPerSeconds);
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
