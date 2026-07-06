using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class HeadbuttTalentController : ITalentController
    {
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        private SpinPlayerCommand _spinPlayerCommand;

        private ushort _casterPlayerId;
        private bool _isCharging;
        private int _chargeStartTick;
        private float _chargeFraction;
        private Vector2 _dashDirection;
        private int _dashStartTick;
        private bool _hasHitEnemy;

        public TalentType TalentType => TalentType.Headbutt;

        private bool IsDashing
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public HeadbuttTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator,
            NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _spinPlayerCommand = _commandFactory.CreateCommandVoid<SpinPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed,
            bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!_isCharging && !IsDashing)
            {
                if (!wasTalentInputDownThisTick) return;

                var isOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();
                if (isOnCooldown) return;

                _isCharging = true;
                _chargeStartTick = tick;
                casterPlayerState.Spaceship.IsEngineOn = false;
                _netEventsDataService.AddActivateHeadbuttChargingNetEvent(tick, _casterPlayerId);
                return;
            }

            if (_isCharging && wasTalentInputReleasedThisTick)
            {
                var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
                var chargedSeconds = (tick - _chargeStartTick) * deltaTime;
                _chargeFraction = Mathf.Clamp01(chargedSeconds / config.MaxChargeDurationSeconds);
                _dashDirection = casterPlayerState.Spaceship.Transform.Direction;

                casterPlayerState.Spaceship.Transform.Velocity += _dashDirection * config.MaxChargeForce * _chargeFraction;
                casterPlayerState.Spaceship.IsEngineOn = true;

                _isCharging = false;
                IsDashing = true;
                _dashStartTick = tick;
                _hasHitEnemy = false;

                _physicsSimulator.EnablePlayerDashCollision(_casterPlayerId);
                _netEventsDataService.AddPerformHeadbuttDashNetEvent(tick, _casterPlayerId);
            }
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (_isCharging)
            {
                var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                if (casterPlayerState.Spaceship.IsSpinned)
                {
                    StopIfActive(tick);
                }
                return;
            }

            if (!IsDashing) return;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
            var dashWindow = Mathf.Lerp(config.MinDashWindowSeconds, config.MaxDashWindowSeconds, _chargeFraction);
            var elapsed = (tick - _dashStartTick) * deltaTime;

            if (elapsed >= dashWindow)
            {
                DeactivateTalent(tick);
            }
        }

        public void HitEnemy(ushort enemyId, int tick)
        {
            if (!IsDashing || _hasHitEnemy) return;

            _hasHitEnemy = true;
            var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var enemyPlayerState = _matchDataService.SimulationState.GetPlayerById(enemyId);

            enemyPlayerState.Spaceship.Transform.Velocity += _dashDirection * config.EnemyPushForce;
            _spinPlayerCommand.SetPlayer(enemyId).SetSpinAmount(config.EnemySpinAmount).SetTick(tick).Execute();
            casterPlayerState.Spaceship.Transform.Velocity *= config.CasterVelocityDamping;

            _netEventsDataService.AddHeadbuttHitEnemyNetEvent(tick, _casterPlayerId, enemyId);
            DeactivateTalent(tick);
        }

        public void StopIfActive(int tick)
        {
            if (_isCharging)
            {
                _isCharging = false;
                var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                if (casterPlayerState.Spaceship.IsAlive)
                    casterPlayerState.Spaceship.IsEngineOn = true;
                DeactivateTalent(tick);
                return;
            }

            if (IsDashing)
            {
                DeactivateTalent(tick);
            }
        }

        private void DeactivateTalent(int tick)
        {
            _isCharging = false;
            IsDashing = false;
            _physicsSimulator.DisablePlayerDashCollision(_casterPlayerId);

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Headbutt, out int talentIndex))
            {
                LogService.LogError($"No Headbutt talent found for player id {_casterPlayerId}");
                return;
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateHeadbuttTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
        }

        public void ResetData()
        {
            _isCharging = false;
            IsDashing = false;
        }
    }
}
