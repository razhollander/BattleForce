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
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly Dictionary<int, PlayerPowerUpControllers> _powerUpControllersPerPlayer;
        private readonly ConcurrentPool<PlayerPowerUpControllers> _powerUpControllersPool;
        private readonly Dictionary<int, int> _grantingPhaseEndTickPerPlayer;
        private readonly Dictionary<int, PowerUpType> _pendingGrantedPowerUpPerPlayer;

        public PlayersPowerUpsManager(NetworkConfig networkConfig, IMatchDataService matchDataService, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, SharedGamePlayConfig sharedGamePlayConfig, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
            _powerUpControllersPerPlayer = new Dictionary<int, PlayerPowerUpControllers>(networkConfig.MaxCap.ConcurrentPlayers);
            _powerUpControllersPool = new ConcurrentPool<PlayerPowerUpControllers>(() => new PlayerPowerUpControllers(matchDataService, netEventsDataService, networkConfig, gamePlayConfigService, commandFactory), networkConfig.MaxCap.ConcurrentPlayers);
            _grantingPhaseEndTickPerPlayer = new Dictionary<int, int>(networkConfig.MaxCap.ConcurrentPlayers);
            _pendingGrantedPowerUpPerPlayer = new Dictionary<int, PowerUpType>(networkConfig.MaxCap.ConcurrentPlayers);
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
            _grantingPhaseEndTickPerPlayer.Remove(playerId);
            _pendingGrantedPowerUpPerPlayer.Remove(playerId);
        }

        public bool TryGrantRandomPowerUp(ushort playerId, int tick)
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            var spaceship = playerState.Spaceship;
            var doesAlreadyHavePowerUp = spaceship.CurrentPowerUp != PowerUpType.None || spaceship.IsCurrentlyInGrantingPowerUpPhase;
            if (doesAlreadyHavePowerUp)
            {
                return false;
            }

            var grantedPowerUp = GetRandomObtainablePowerUp();
            var grantingPhaseEndTick = TickUtils.GetTickPassedAfterDuration(tick, _sharedGamePlayConfig.PowerUps.PowerUpObtainDelayInSeconds, _networkConfig.DeltaTime);

            spaceship.IsCurrentlyInGrantingPowerUpPhase = true;
            _grantingPhaseEndTickPerPlayer[playerId] = grantingPhaseEndTick;
            _pendingGrantedPowerUpPerPlayer[playerId] = grantedPowerUp;
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
            if (currentPowerUp == PowerUpType.None)
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
                playerState.Spaceship.CurrentPowerUp = PowerUpType.None;
                _netEventsDataService.AddPlayerPowerUpChangedNetEvent(tick, playerId, PowerUpType.None);
            }
        }

        public void OnTick(int tick)
        {
            foreach (var kvp in _powerUpControllersPerPlayer)
            {
                var playerId = (ushort)kvp.Key;
                var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
                var spaceship = playerState.Spaceship;

                var shouldFinalizeGrantingPhase = spaceship.IsCurrentlyInGrantingPowerUpPhase && tick >= _grantingPhaseEndTickPerPlayer[playerId];
                if (shouldFinalizeGrantingPhase)
                {
                    var grantedPowerUp = _pendingGrantedPowerUpPerPlayer[playerId];
                    spaceship.IsCurrentlyInGrantingPowerUpPhase = false;
                    spaceship.CurrentPowerUp = grantedPowerUp;
                    _netEventsDataService.AddEndPowerUpGrantingPhaseNetEvent(tick, playerId, grantedPowerUp);
                    _netEventsDataService.AddPlayerPowerUpChangedNetEvent(tick, playerId, grantedPowerUp);
                }

                var wasActive = spaceship.IsPowerUpCurrentlyActive;
                kvp.Value.OnTick(tick);

                if (wasActive && !spaceship.IsPowerUpCurrentlyActive)
                {
                    spaceship.CurrentPowerUp = PowerUpType.None;
                    _netEventsDataService.AddPlayerPowerUpChangedNetEvent(tick, playerId, PowerUpType.None);
                }
            }
        }

        public bool IsPerformInProgressForPlayer(ushort playerId)
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

            _grantingPhaseEndTickPerPlayer.Clear();
            _pendingGrantedPowerUpPerPlayer.Clear();
        }

        public bool IsPlayerAimingPowerUp(ushort playerId)
        {
            return false; // no current power-up uses aiming; SonicSlap is instant
        }

        private PowerUpType GetRandomObtainablePowerUp()
        {
            var obtainablePowerUps = _gamePlayConfigService.GamePlayConfig.PowerUps.ObtainablePowerUps;
            var randomIndex = RNG.NextInt(0, obtainablePowerUps.Length);
            return obtainablePowerUps[randomIndex];
        }
    }
}
