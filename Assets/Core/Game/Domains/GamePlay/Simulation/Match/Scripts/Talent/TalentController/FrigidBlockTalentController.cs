using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class FrigidBlockTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ICommandFactory _commandFactory;
        private ShootFrigidBlockForPlayerCommand _shootFrigidBlockForPlayerCommand;

        public TalentType TalentType => TalentType.FrigidBlock;

        private bool IsCurrentlyAiming
        {
            get => _matchDataService.SimulationState.GetIsTalentAimingForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyAimingForPlayer(_casterPlayerId, TalentType, value);
        }

        public FrigidBlockTalentController(IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _commandFactory = commandFactory;
        }

        public void InitEntryPoint()
        {
            _shootFrigidBlockForPlayerCommand = _commandFactory.CreateCommandVoid<ShootFrigidBlockForPlayerCommand>();
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
                    casterPlayerState.Spaceship.AssistArrowType = Shared.Scripts.Enums.PlayerAssistArrowType.AimArrow;
                }
            }

            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }

            casterPlayerState.Spaceship.AssistArrowType = Shared.Scripts.Enums.PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType, out int talentIndex))
            {
                LogService.LogError($"No FrigidBlock talent found for player id {_casterPlayerId}");
                return;
            }

            var direction = casterPlayerState.Spaceship.TalentsState.AimDirection;
            var config = _gamePlayConfigService.GamePlayConfig.Talents.FrigidBlockTalentConfig;
            var blockThickness = _sharedGamePlayConfig.FrigidBlockSize.y;
            var spawnOffset = casterPlayerState.Spaceship.Transform.Radius + blockThickness * 0.5f + config.SpawnGapFromCaster;
            var position = casterPlayerState.Spaceship.Transform.Position + direction * spawnOffset;

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _shootFrigidBlockForPlayerCommand
                .SetCasterPlayerId(_casterPlayerId)
                .SetPosition(position)
                .SetDirection(direction)
                .SetTick(tick)
                .SetCooldownEndTick(cooldownEndTick)
                .Execute();
        }

        public void StopIfActive(int tick)
        {
        }

        public void OnTick(int tick, float deltaTime)
        {
        }

        public void ResetData()
        {
            IsCurrentlyAiming = false;
        }
    }
}
