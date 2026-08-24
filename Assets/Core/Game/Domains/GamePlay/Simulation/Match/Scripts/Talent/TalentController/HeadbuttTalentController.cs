using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
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
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ICommandFactory _commandFactory;
        private TrySpinPlayerCommand _trySpinPlayerCommand;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;

        private ushort _casterPlayerId;
        private HeadButtPhaseType _phase;
        private int _chargeStartTick;
        private float _chargeFraction;
        private Vector2 _dashDirection;
        private int _dashStartTick;
        private bool _hasHitEnemy;

        public TalentType TalentType => TalentType.Headbutt;

        // The talent counts as active from the first charging tick through the end of the dash.
        private bool IsTalentActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public bool IsCharging => _phase == HeadButtPhaseType.Charging;

        public HeadbuttTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator,
            NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _trySpinPlayerCommand = _commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed,
            bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (_phase == HeadButtPhaseType.None)
            {
                if (!wasTalentInputDownThisTick) return;

                var isOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();
                if (isOnCooldown) return;

                _phase = HeadButtPhaseType.Charging;
                IsTalentActive = true;
                _chargeStartTick = tick;
                casterPlayerState.Spaceship.IsEngineOn = false;
                _netEventsDataService.AddActivateHeadbuttChargingNetEvent(tick, _casterPlayerId);
                return;
            }

            if (_phase == HeadButtPhaseType.Charging && wasTalentInputReleasedThisTick)
            {
                var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
                var chargedSeconds = (tick - _chargeStartTick) * deltaTime;
                _chargeFraction = Mathf.Clamp01(chargedSeconds / _sharedGamePlayConfig.HeadbuttMaxChargeDurationSeconds);
                _dashDirection = casterPlayerState.Spaceship.Transform.Direction;

                _tryAddForceToPlayerCommand.SetPlayerId(_casterPlayerId).SetForce(_dashDirection * config.MaxChargeForce * _chargeFraction).ShouldTurnOffEngine(false).Execute();
                casterPlayerState.Spaceship.IsEngineOn = true;

                _phase = HeadButtPhaseType.Dashing;
                _dashStartTick = tick;
                _hasHitEnemy = false;

                _physicsSimulator.EnablePlayerToCollideWithPlayers(_casterPlayerId);
                _netEventsDataService.AddPerformHeadbuttDashNetEvent(tick, _casterPlayerId);
            }
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (_phase == HeadButtPhaseType.Charging)
            {
                return;
            }

            if (_phase != HeadButtPhaseType.Dashing) return;

            var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
            var dashWindow = Mathf.Lerp(config.MinSecondsInDash, config.MaxSecondsInDash, _chargeFraction);
            var elapsed = (tick - _dashStartTick) * deltaTime;

            if (elapsed >= dashWindow)
            {
                DeactivateTalent(tick);
            }
        }

        public void HitEnemy(ushort enemyId, int tick)
        {
            if (_phase != HeadButtPhaseType.Dashing || _hasHitEnemy) return;

            _hasHitEnemy = true;
            var config = _gamePlayConfigService.GamePlayConfig.Talents.HeadbuttTalentConfig;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            _trySpinPlayerCommand.SetPlayer(enemyId).SetSpinAmount(config.EnemySpinAmount).SetTick(tick).Execute();
            _tryAddForceToPlayerCommand.SetPlayerId(enemyId).SetForce(_dashDirection * config.EnemyPushForce).ShouldTurnOffEngine(true).Execute();
            casterPlayerState.Spaceship.Transform.Velocity = Vector2.Zero;

            _netEventsDataService.AddHeadbuttHitEnemyNetEvent(tick, _casterPlayerId, enemyId);
            DeactivateTalent(tick);
        }

        // Smashing a mole bounces the caster straight back: velocity and facing direction both flip 180 degrees, but it does not consume the dash's single enemy hit.
        public void HitMole()
        {
            if (_phase != HeadButtPhaseType.Dashing)
            {
                return;
            }

            ref var casterTransform = ref _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).Spaceship.Transform;
            casterTransform.Velocity = -casterTransform.Velocity;
            casterTransform.Direction = -casterTransform.Direction;
        }

        public void StopIfActive(int tick)
        {
            if (_phase == HeadButtPhaseType.Charging)
            {
                _phase = HeadButtPhaseType.None;
                var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
                if (casterPlayerState.Spaceship.IsAlive)
                {
                    casterPlayerState.Spaceship.IsEngineOn = true;
                }
                DeactivateTalent(tick);
                return;
            }

            if (_phase == HeadButtPhaseType.Dashing)
            {
                DeactivateTalent(tick);
            }
        }

        private void DeactivateTalent(int tick)
        {
            _phase = HeadButtPhaseType.None;
            IsTalentActive = false;
            _physicsSimulator.DisablePlayerToCollideWithPlayers(_casterPlayerId);

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
            _phase = HeadButtPhaseType.None;
            IsTalentActive = false;
        }
    }
}
