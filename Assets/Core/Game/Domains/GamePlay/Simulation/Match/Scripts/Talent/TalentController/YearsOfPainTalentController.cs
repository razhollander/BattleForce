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
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;
        private readonly SpinPlayerCommand _spinPlayerCommand;

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
        
        public YearsOfPainTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
            _spinPlayerCommand = commandFactory.CreateCommandVoid<SpinPlayerCommand>();
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

            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var offset = casterPlayerState.Spaceship.Transform.Radius;
            var config = _gamePlayConfig.Talents.YearsOfPainTalentConfig;
            var colliderSize = config.RectangleColliderSize.ToNumericsVector2();
            var center = casterPlayerState.Spaceship.Transform.Position + (direction * (offset + colliderSize.Y * 0.5f));
            var angleRadians = direction.ToAngleRadians();
            ushort hitEnemyId = 0;

            var didHitEnemy = _physicsSimulator.RectangleCastOnPlayers(center, colliderSize, angleRadians, (short) casterPlayerState.TeamId, out var hitBodyData);
            if (didHitEnemy)
            {
                hitEnemyId = hitBodyData.Id;
                ApplyEffectToEnemyPhysics(tick, hitEnemyId, casterPlayerState);
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddActivateYearsOfPainTalentNetEventS2C(tick, _casterPlayerId, direction, cooldownEndTick, didHitEnemy, hitEnemyId);
        }

        private void ApplyEffectToEnemyPhysics(int tick, ushort enemyId, PlayerStateS2C casterPlayerState)
        {
            var config = _gamePlayConfig.Talents.YearsOfPainTalentConfig;
            var hitEnemyPlayer = _matchDataService.SimulationState.GetPlayerById(enemyId);
            var pushForce = config.PushForce;
            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;

            var forceToEnemy = direction * pushForce;
            hitEnemyPlayer.Spaceship.Transform.Velocity += forceToEnemy;

            var randomSpin = RNG.NextFloat(config.MinSpin, config.MaxSpin);
            _spinPlayerCommand.SetPlayer(hitEnemyPlayer.Id).SetSpinAmount(randomSpin).SetTick(tick).Execute();
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
