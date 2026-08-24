using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class WaterGunTalentController : ITalentController
    {
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        private PushScoreGateCommand _pushScoreGateCommand;

        private ushort _casterPlayerId;
        private int _startTick;

        public TalentType TalentType => TalentType.WaterGun;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public WaterGunTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig,
            ICommandFactory commandFactory)
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
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
            _pushScoreGateCommand = _commandFactory.CreateCommandVoid<PushScoreGateCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed,
            bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (IsCurrentlyActive)
            {
                DeactivateTalent(tick);
                return;
            }

            var isOnCooldown = !casterPlayerState.Spaceship.TalentsState.TryGetTalentByType(TalentType, out var talentState) || talentState.IsOnCooldown();
            if (isOnCooldown)
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;
            _netEventsDataService.AddActivateWaterGunTalentNetEvent(tick, _casterPlayerId);
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

            var config = _gamePlayConfigService.GamePlayConfig.Talents.WaterGunTalentConfig;
            var elapsedSeconds = (tick - _startTick) * deltaTime;

            if (elapsedSeconds >= config.DurationInSeconds)
            {
                DeactivateTalent(tick);
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var aimDirection = casterPlayerState.Spaceship.AimDirection;
            var offset = casterPlayerState.Spaceship.Transform.Radius;
            var center = casterPlayerState.Spaceship.Transform.Position + aimDirection * offset;

            var didHitEnemy = _physicsSimulator.EllipseCastOnPlayers(
                center, config.ConeRange, aimDirection, config.ConeAngleDegrees,
                (short)casterPlayerState.TeamId, out var hitBodyData);

            if (didHitEnemy)
            {
                _tryAddForceToPlayerCommand.SetPlayerId(hitBodyData.Id).SetForce(aimDirection * config.EnemyPushForcePerTick * deltaTime).ShouldTurnOffEngine(false).Execute();
            }

            _tryAddForceToPlayerCommand.SetPlayerId(_casterPlayerId).SetForce(-aimDirection * config.CasterRecoilForcePerTick * deltaTime).ShouldTurnOffEngine(false).Execute();

            TryPushScoreGatesInCone(center, aimDirection, config.ConeRange, config.ConeAngleDegrees, deltaTime);
        }

        // EllipseCastOnPlayers only sees players, so the stream nudges any score gate whose centre falls inside the cone
        // with a geometric test instead. Applied continuously (per tick), so the gate drifts while the stream is on it.
        private void TryPushScoreGatesInCone(Vector2 center, Vector2 aimDirection, float coneRange, float coneAngleDegrees, float deltaTime)
        {
            var scoreGates = _matchDataService.SimulationState.ScoreGates;
            if (scoreGates.Count == 0)
            {
                return;
            }

            var aimDirectionNormalized = aimDirection.NormalizeSafe();
            var halfConeCosine = System.MathF.Cos((coneAngleDegrees * 0.5f).ToRadians());
            var pushPerTick = _gamePlayConfigService.GamePlayConfig.GatePass.WaterGunPushImpulsePerSecond * deltaTime;

            for (int i = 0; i < scoreGates.Count; i++)
            {
                var scoreGate = scoreGates[i];
                var toGate = scoreGate.Position - center;
                var distance = toGate.Length();
                if (distance > coneRange || distance <= 0f)
                {
                    continue;
                }

                var isInsideCone = Vector2.Dot(aimDirectionNormalized, toGate / distance) >= halfConeCosine;
                if (!isInsideCone)
                {
                    continue;
                }

                _pushScoreGateCommand
                    .SetScoreGateId(scoreGate.Id)
                    .SetImpulse(aimDirectionNormalized * pushPerTick)
                    .SetWorldContactPoint(scoreGate.Position)
                    .SetExtraSpinImpulse(0f)
                    .Execute();
            }
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType, out int talentIndex))
            {
                LogService.LogError($"No WaterGun talent found for player id {_casterPlayerId}");
                return;
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateWaterGunTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
