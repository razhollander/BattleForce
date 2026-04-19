using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SentryGunTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private int _startTick;

        private readonly IOverrideableNetEventsService _overrideableNetEventsService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly TryPerformShootForPlayerIfNotOnCooldownCommand _tryPerformShootForPlayerIfNotOnCooldownCommand;

        public TalentType TalentType => TalentType.SentryGun;
        private bool IsCurrentlyActive
        {
            get
            {
                return _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            }
            set
            {
                _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
            }
        }
        
        public SentryGunTalentController(INetEventsDataService netEventsDataService, IOverrideableNetEventsService overrideableNetEventsService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _tryPerformShootForPlayerIfNotOnCooldownCommand = commandFactory.CreateCommandVoid<TryPerformShootForPlayerIfNotOnCooldownCommand>();
            _overrideableNetEventsService = overrideableNetEventsService;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !wasTalentInputDownThisTick)
            {
                if (wasTalentInputDownThisTick)
                {
                    DeactivateTalent(tick);
                }
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();

            if (isOnCooldown)
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;


            casterPlayerState.Spaceship.IsEngineOn = false;
            casterPlayerState.Spaceship.Transform.StopMotion();
            ChangePlayerMaxShootCooldown(casterPlayerState, _gamePlayConfig.Talents.SentryGunTalentConfig.ShootCooldownMultiplier);
            _netEventsDataService.AddActivateSentryGunTalentNetEvent(tick, _casterPlayerId);
            _overrideableNetEventsService.OverridePlayerMaxShootCooldownChangedEvent(tick, _casterPlayerId, casterPlayerState.Spaceship.Shoot.MaxCooldown, casterPlayerState.Spaceship.Shoot.CooldownSecondsLeft);
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }
            
            _tryPerformShootForPlayerIfNotOnCooldownCommand.SetPlayerId(_casterPlayerId).SetTick(tick).Execute();
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isSpinned = casterPlayerState.Spaceship.IsSpinned;
            var elapsedSeconds = (tick - _startTick) * deltaTime;
            var didSentryGunTimeEnded = elapsedSeconds >= _gamePlayConfig.Talents.SentryGunTalentConfig.DurationInSeconds;
            
            if (isSpinned || didSentryGunTimeEnded)
            {
                DeactivateTalent(tick);
            }
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (casterPlayerState.Spaceship.IsAlive)
            {
                casterPlayerState.Spaceship.IsEngineOn = true;
            }
            
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.SentryGun, out int talentIndex))
            {
                LogService.LogError($"No SentryGun talent found for player id {_casterPlayerId}");
                return;
            }
            
            ref var sentryTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, sentryTalentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            sentryTalentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateSentryGunTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
            ChangePlayerMaxShootCooldown(casterPlayerState, 1f / _gamePlayConfig.Talents.SentryGunTalentConfig.ShootCooldownMultiplier);
            _overrideableNetEventsService.OverridePlayerMaxShootCooldownChangedEvent(tick, _casterPlayerId, casterPlayerState.Spaceship.Shoot.MaxCooldown, casterPlayerState.Spaceship.Shoot.CooldownSecondsLeft);
        }

        private void ChangePlayerMaxShootCooldown(PlayerStateS2C playerState, float multiplyMaxCooldownBy)
        {
            playerState.Spaceship.Shoot.MaxCooldown *= multiplyMaxCooldownBy;
            playerState.Spaceship.Shoot.CooldownSecondsLeft = Mathf.Min(playerState.Spaceship.Shoot.CooldownSecondsLeft, playerState.Spaceship.Shoot.MaxCooldown);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}