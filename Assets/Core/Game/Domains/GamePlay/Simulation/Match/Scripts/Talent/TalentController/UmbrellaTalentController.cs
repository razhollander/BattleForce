using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class UmbrellaTalentController : ITalentController
    {
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;
        private readonly TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        
        private ushort _casterPlayerId;
        private int _startTick;

        public TalentType TalentType => TalentType.Umbrella;
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

        public UmbrellaTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _tryAddForceToPlayerCommand = commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            var isCurrentlyAiming = IsCurrentlyAiming;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isOnCooldown = !casterPlayerState.Spaceship.TalentsState.TryGetTalentByType(TalentType, out var talentState) || talentState.IsOnCooldown();
            if (isOnCooldown)
            {
                return;
            }
            
            if (wasTalentInputDownThisTick)
            {
                if (IsCurrentlyActive)
                {
                    DeactivateTalent(tick);
                    return;
                }

                if (!isCurrentlyAiming)
                {
                    IsCurrentlyAiming = true;
                    casterPlayerState.Spaceship.AssistArrowType = PlayerAssistArrowType.AimArrow;
                }
            }


            if (IsCurrentlyActive)
            {
                return;
            }
            
            if (!wasTalentInputReleasedThisTick || !isCurrentlyAiming)
            {
                return;
            }
            
            casterPlayerState.Spaceship.AssistArrowType = PlayerAssistArrowType.Hidden;
            IsCurrentlyAiming = false;
            IsCurrentlyActive = true;
            casterPlayerState.Spaceship.IsEngineOn = false;
            _startTick = tick;
            _netEventsDataService.AddActivateUmbrellaTalentNetEvent(tick, _casterPlayerId);
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
            var elapsedSeconds = (tick - _startTick) * deltaTime;
            var didTimeEnded = elapsedSeconds >= _gamePlayConfigService.GamePlayConfig.Talents.UmbrellaTalentConfig.DurationInSeconds;

            if (didTimeEnded)
            {
                DeactivateTalent(tick);
                return;
            }

            var aimDirection = casterPlayerState.Spaceship.AimDirection;
            var force = aimDirection * _gamePlayConfigService.GamePlayConfig.Talents.UmbrellaTalentConfig.VelocityGainPerTick * deltaTime;
            _tryAddForceToPlayerCommand.SetPlayerId(_casterPlayerId).SetForce(force).Execute();
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Umbrella, out int talentIndex))
            {
                LogService.LogError($"No Umbrella talent found for player id {_casterPlayerId}");
                return;
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateUmbrellaTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
