using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class YearsOfPainTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private TrySpinPlayerCommand _trySpinPlayerCommand;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        private TryHitMoleCommand _tryHitMoleCommand;
        private PushScoreGateCommand _pushScoreGateCommand;
        private readonly ICommandFactory _commandFactory;

        public TalentType TalentType => TalentType.YearsOfPain;

        private bool IsCurrentlyAiming
        {
            get
            {
                return _matchDataService.SimulationState.GetIsTalentAimingForPlayer(_casterPlayerId, TalentType);
            }
            set
            {
                _matchDataService.SimulationState.SetIsTalentCurrentlyAimingForPlayer(_casterPlayerId, TalentType, value);
            }
        }
        
        public YearsOfPainTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, ICommandFactory commandFactory)
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
            _trySpinPlayerCommand = _commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _tryAddForceToPlayerCommand = _commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
            _tryHitMoleCommand = _commandFactory.CreateCommandVoid<TryHitMoleCommand>();
            _pushScoreGateCommand = _commandFactory.CreateCommandVoid<PushScoreGateCommand>();
        }
        
        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var isCurrentlyAiming = IsCurrentlyAiming;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            var isCurrentSelectedTalentOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();
            if (isCurrentSelectedTalentOnCooldown)
            {
                return;
            }

            if (wasTalentInputDownThisTick)
            {
                if (!isCurrentlyAiming)
                {
                    IsCurrentlyAiming = true;
                    casterPlayerState.Spaceship.AssistArrowType = Core.Game.Domains.GamePlay.Shared.Scripts.Enums.PlayerAssistArrowType.AimArrow;
                }
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = Core.Game.Domains.GamePlay.Shared.Scripts.Enums.PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType, out int talentIndex))
            {
                LogService.LogError($"No Years Of Pain talent found for player id {_casterPlayerId}");
                return;
            }

            var direction = casterPlayerState.Spaceship.AimDirection;
            var offset = casterPlayerState.Spaceship.Transform.Radius;
            var config = _gamePlayConfigService.GamePlayConfig.Talents.YearsOfPainTalentConfig;
            var colliderSize = config.RectangleColliderSize.ToNumericsVector2();
            var center = casterPlayerState.Spaceship.Transform.Position + (direction * (offset + colliderSize.Y * 0.5f));
            var angleRadians = direction.ToAngleRadians();
            ushort hitEnemyId = 0;
            var didHitEnemy = false;

            // A hit enemy takes priority; a mole is only whacked when none was inside the rectangle. Moles only exist in the WhacAMole stage, so elsewhere the mole type simply never matches.
            if (_physicsSimulator.RectangleCastByPriority(center, colliderSize, angleRadians, (short) casterPlayerState.TeamId, PhysicsBodyType.PlayerSpaceship, PhysicsBodyType.Mole, out var hitBodyData))
            {
                if (hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship)
                {
                    didHitEnemy = true;
                    hitEnemyId = hitBodyData.Id;
                    ApplyEffectToEnemyPhysics(tick, hitEnemyId, casterPlayerState);
                }
                else
                {
                    _tryHitMoleCommand
                        .SetMoleId(hitBodyData.Id)
                        .SetByPlayerId(_casterPlayerId)
                        .SetByTeamId(casterPlayerState.TeamId)
                        .SetProcessedTick(tick)
                        .Execute();
                }
            }

            // Independently of the enemy/mole hit, the rectangle shoves and spins any score gate it covers.
            if (_matchDataService.SimulationState.ScoreGates.Count > 0
                && _physicsSimulator.RectangleCastByPriority(center, colliderSize, angleRadians, (short) casterPlayerState.TeamId, PhysicsBodyType.ScoreGate, PhysicsBodyType.ScoreGate, out var hitGateData)
                && hitGateData.PhysicsBodyType == PhysicsBodyType.ScoreGate)
            {
                PushScoreGate(hitGateData.Id, direction);
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddActivateYearsOfPainTalentNetEventS2C(tick, _casterPlayerId, direction, cooldownEndTick, didHitEnemy, hitEnemyId);
        }

        private void PushScoreGate(ushort scoreGateId, System.Numerics.Vector2 direction)
        {
            var gatePosition = _matchDataService.SimulationState.GetScoreGateById(scoreGateId).Position;
            var gatePassConfig = _gamePlayConfigService.GamePlayConfig.GatePass;

            _pushScoreGateCommand
                .SetScoreGateId(scoreGateId)
                .SetImpulse(direction * gatePassConfig.YearsOfPainPushImpulse)
                .SetWorldContactPoint(gatePosition)
                .SetExtraSpinImpulse(gatePassConfig.YearsOfPainSpinImpulse)
                .Execute();
        }

        private void ApplyEffectToEnemyPhysics(int tick, ushort enemyId, PlayerStateS2C casterPlayerState)
        {
            var config = _gamePlayConfigService.GamePlayConfig.Talents.YearsOfPainTalentConfig;
            var hitEnemyPlayer = _matchDataService.SimulationState.GetPlayerById(enemyId);
            var pushForce = config.PushForce;
            var direction = casterPlayerState.Spaceship.AimDirection;

            var forceToEnemy = direction * pushForce;
            var randomSpin = RNG.NextFloat(config.MinSpin, config.MaxSpin);
            _trySpinPlayerCommand.SetPlayer(hitEnemyPlayer.Id).SetSpinAmount(randomSpin).SetTick(tick).Execute();
            _tryAddForceToPlayerCommand.SetForce(forceToEnemy).SetPlayerId(enemyId).ShouldTurnOffEngine(true).Execute();
        }

        public void StopIfActive(int tick)
        {

        }

        public void OnTick(int tick, float deltaTime)
        {

        }

        public void ResetData()
        {
        }
    }
}
