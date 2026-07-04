using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public class PlayersPowerUpsManager : IPlayersPowerUpsManager
    {
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly Dictionary<int, PlayerPowerUpControllers> _powerUpControllersPerPlayer;
        private readonly ConcurrentPool<PlayerPowerUpControllers> _powerUpControllersPool;
        private readonly Dictionary<int, PowerUpInGrantingPhase> _powerUpInGrantingPhasePerPlayer;

        public PlayersPowerUpsManager(NetworkConfig networkConfig, IMatchDataService matchDataService, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, SharedGamePlayConfig sharedGamePlayConfig, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _powerUpControllersPerPlayer = new Dictionary<int, PlayerPowerUpControllers>(networkConfig.MaxCap.ConcurrentPlayers);
            _powerUpControllersPool = new ConcurrentPool<PlayerPowerUpControllers>(() => new PlayerPowerUpControllers(matchDataService, netEventsDataService, networkConfig, gamePlayConfigService, commandFactory), networkConfig.MaxCap.ConcurrentPlayers);
            _powerUpInGrantingPhasePerPlayer = new Dictionary<int, PowerUpInGrantingPhase>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddPlayer(ushort playerId)
        {
            var powerUpControllers = _powerUpControllersPool.Get();
            powerUpControllers.SetCasterId(playerId);
            _powerUpControllersPerPlayer.Add(playerId, powerUpControllers);
        }

        public void RemovePlayer(ushort playerId)
        {
            _powerUpControllersPool.Return(_powerUpControllersPerPlayer[playerId]);
            _powerUpControllersPerPlayer.Remove(playerId);
            _powerUpInGrantingPhasePerPlayer.Remove(playerId);
        }

        public bool TryGrantPowerUp(ushort playerId, PowerUpType grantedPowerUpType, int tick)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var spaceship = playerState.Spaceship;
            var doesAlreadyHavePowerUp = spaceship.CurrentPowerUp != PowerUpType.None || spaceship.IsCurrentlyInGrantingPowerUpPhase;
            if (doesAlreadyHavePowerUp)
            {
                return false;
            }

            var grantingPhaseEndTick = TickUtils.GetTickPassedAfterDuration(tick, _sharedGamePlayConfig.PowerUps.PowerUpObtainDelayInSeconds, _networkConfig.DeltaTime);

            spaceship.IsCurrentlyInGrantingPowerUpPhase = true;
            _powerUpInGrantingPhasePerPlayer[playerId] = new PowerUpInGrantingPhase(grantingPhaseEndTick, grantedPowerUpType);
            _netEventsDataService.AddStartPowerUpGrantingPhaseNetEvent(tick, playerId);
            return true;
        }

        public void ProcessPowerUpInput(ushort playerId, int tick, bool wasPowerUpInputDownThisTick)
        {
            if (!wasPowerUpInputDownThisTick)
            {
                return;
            }

            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var currentPowerUp = playerState.Spaceship.CurrentPowerUp;
            var doesHaveAnyPowerUp = currentPowerUp != PowerUpType.None;
            if (!doesHaveAnyPowerUp)
            {
                return;
            }

            var isAimingTalent = playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) && selectedTalent.IsCurrentlyAiming;
            if (isAimingTalent)
            {
                return;
            }

            var controllers = _powerUpControllersPerPlayer[playerId];
            controllers.Perform(currentPowerUp, tick);

            if (!playerState.Spaceship.IsPowerUpCurrentlyActive)
            {
                ResetPlayerPowerUp(playerState, tick);
            }
        }

        public void OnTick(int tick)
        {
            foreach (var kvp in _powerUpControllersPerPlayer)
            {
                var playerId = (ushort)kvp.Key;
                var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                var spaceship = playerState.Spaceship;

                var shouldFinalizeGrantingPhase = spaceship.IsCurrentlyInGrantingPowerUpPhase && tick >= _powerUpInGrantingPhasePerPlayer[playerId].GrantingPhaseEndTick;
                if (shouldFinalizeGrantingPhase)
                {
                    var grantedPowerUp = _powerUpInGrantingPhasePerPlayer[playerId].PowerUpType;
                    spaceship.IsCurrentlyInGrantingPowerUpPhase = false;
                    spaceship.CurrentPowerUp = grantedPowerUp;
                    _netEventsDataService.AddEndPowerUpGrantingPhaseNetEvent(tick, playerId, grantedPowerUp);
                    _netEventsDataService.AddPlayerPowerUpChangedNetEvent(tick, playerId, grantedPowerUp);
                }

                var wasActive = spaceship.IsPowerUpCurrentlyActive;
                kvp.Value.OnTick(tick);
                var wasDeactivatedInThisTick = wasActive && !spaceship.IsPowerUpCurrentlyActive;

                if (wasDeactivatedInThisTick)
                {
                    ResetPlayerPowerUp(playerState, tick);
                }
            }
        }

        private void ResetPlayerPowerUp(PlayerStateS2C spaceship, int tick)
        {
            spaceship.Spaceship.CurrentPowerUp = PowerUpType.None;
            _netEventsDataService.AddPlayerPowerUpChangedNetEvent(tick, spaceship.Id, PowerUpType.None);
        }

        public bool IsPowerUpActiveForPlayer(ushort playerId)
        {
            if (!_powerUpControllersPerPlayer.ContainsKey(playerId))
                return false;

            return _matchDataService.SimulationState.GetIsPowerUpCurrentlyActiveForPlayer(playerId);
        }

        public void RemoveAllPowerUps()
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                playerState.Spaceship.CurrentPowerUp = PowerUpType.None;
                playerState.Spaceship.IsCurrentlyInGrantingPowerUpPhase = false;
            }

            _powerUpInGrantingPhasePerPlayer.Clear();
        }
    }
}
