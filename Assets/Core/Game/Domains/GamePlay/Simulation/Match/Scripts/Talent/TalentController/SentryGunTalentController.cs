using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SentryGunTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private int _startTick;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        public TalentType TalentType => TalentType.SentryGun;
        public bool IsCurrentlyActive { get; private set; }

        public SentryGunTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;

            var sentryConfig = _gamePlayConfig.Talents.SentryGunTalentConfig;

            casterPlayerState.Spaceship.IsEngineOn = false;
            casterPlayerState.Spaceship.Transform.Velocity = Vector2.Zero;
            casterPlayerState.Spaceship.Shoot.MaxCooldown *= sentryConfig.ShootCooldownMultiplier;
            casterPlayerState.Spaceship.Shoot.CooldownSecondsLeft = Mathf.Min(casterPlayerState.Spaceship.Shoot.MaxCooldown, casterPlayerState.Spaceship.Shoot.CooldownSecondsLeft);
            _netEventsDataService.AddActivateSentryGunTalentNetEvent(tick, _casterPlayerId);
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

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var sentryConfig = _gamePlayConfig.Talents.SentryGunTalentConfig;

            casterPlayerState.Spaceship.IsEngineOn = false;
            casterPlayerState.Spaceship.Transform.Velocity = Vector2.Zero;

            bool isDead = !casterPlayerState.Spaceship.IsAlive;
            bool switchedTalents = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().TalentType != TalentType.SentryGun;
            bool isSpinned = casterPlayerState.Spaceship.Transform.AngularVelocity != 0;

            if (isDead || switchedTalents || isSpinned)
            {
                DeactivateTalent(tick);
                return;
            }

            var elapsedSeconds = (tick - _startTick) * deltaTime;
            if (elapsedSeconds >= sentryConfig.DurationInSeconds)
            {
                DeactivateTalent(tick);
            }
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var sentryConfig = _gamePlayConfig.Talents.SentryGunTalentConfig;

            if (casterPlayerState.Spaceship.IsAlive)
            {
                casterPlayerState.Spaceship.IsEngineOn = true;
                casterPlayerState.Spaceship.Shoot.MaxCooldown /= sentryConfig.ShootCooldownMultiplier;
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
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}