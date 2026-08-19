using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    /// <summary>
    /// Keeps a mole hole shut for a while after the mole that came out of it is gone. Without it a mole can be hidden
    /// and another one spawned from the same hole on the same tick, and the presentation domain - which tracks holes
    /// rather than moles - has no way to tell which of the two each net event belongs to.
    /// </summary>
    public class MolesSpawnCooldownService : IMolesSpawnCooldownService
    {
        private const int NO_COOLDOWN_TICK = 0; // a hole that never held a mole this stage is free right away
        private const int FIRST_MOLE_HOLE_ID = 1; // zero is kept free so it can mean "no mole hole", so the array holds one slot more than the cap

        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;

        private int[] _cooldownEndTickPerMoleHoleId = Array.Empty<int>();

        public MolesSpawnCooldownService(ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
        }

        public void InitEntryPoint()
        {
            _cooldownEndTickPerMoleHoleId = new int[_networkConfig.MaxCap.MoleHoles + FIRST_MOLE_HOLE_ID];
        }

        public void ClearAllCooldowns()
        {
            Array.Clear(_cooldownEndTickPerMoleHoleId, 0, _cooldownEndTickPerMoleHoleId.Length);
        }

        public void RegisterMoleHoleToBeOnCooldown(ushort moleHoleId, int tick)
        {
            if (moleHoleId >= _cooldownEndTickPerMoleHoleId.Length)
            {
                LogService.LogError($"Mole hole {moleHoleId} is above the {nameof(NetworkConfig.MaxCap.MoleHoles)} cap!");
                return;
            }

            var cooldownSeconds = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleHoleReuseCooldownSeconds;
            _cooldownEndTickPerMoleHoleId[moleHoleId] = tick + (int)MathF.Ceiling(cooldownSeconds * _networkConfig.TicksPerSeconds);
        }

        public bool IsMoleHoleOnCooldown(ushort moleHoleId, int tick)
        {
            if (moleHoleId >= _cooldownEndTickPerMoleHoleId.Length)
            {
                return false;
            }

            var cooldownEndTick = _cooldownEndTickPerMoleHoleId[moleHoleId];
            return cooldownEndTick != NO_COOLDOWN_TICK && tick < cooldownEndTick;
        }
    }
}
