using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class FrozenTalentController : ITalentController
    {
        private ushort _casterPlayerId;
        private int _startTick;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        private readonly IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private UpdatePlayerLavaExposureCommand _updatePlayerLavaExposureCommand;
        private TryCollidePlayerWithOverlappingSpikeCommand _tryCollidePlayerWithOverlappingSpikeCommand;

        public TalentType TalentType => TalentType.Frozen;

        private bool IsCurrentlyActive
        {
            get => _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            set => _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
        }

        public FrozenTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig, ICommandFactory commandFactory,
            IPlayersInLavaTrackerService playersInLavaTrackerService)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
            _playersInLavaTrackerService = playersInLavaTrackerService;
        }

        public void InitEntryPoint()
        {
            _updatePlayerLavaExposureCommand = _commandFactory.CreateCommandVoid<UpdatePlayerLavaExposureCommand>();
            _tryCollidePlayerWithOverlappingSpikeCommand = _commandFactory.CreateCommandVoid<TryCollidePlayerWithOverlappingSpikeCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }
            
            if (IsCurrentlyActive)
            {
                DeactivateTalent(tick);
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            ActivateTalent(tick, casterPlayerState);
        }

        private void ActivateTalent(int tick, PlayerStateS2C casterPlayerState)
        {
            IsCurrentlyActive = true;
            _startTick = tick;
            casterPlayerState.Spaceship.IsEngineOn = false;

            _netEventsDataService.AddActivateFrozenTalentNetEvent(tick, _casterPlayerId);
            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }
            
            var elapsedSeconds = (tick - _startTick) * deltaTime;
            if (elapsedSeconds >= _gamePlayConfigService.GamePlayConfig.Talents.FrozenTalentConfig.DurationInSeconds)
            {
                DeactivateTalent(tick);
            }
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (casterPlayerState.Spaceship.IsAlive)
            {
                casterPlayerState.Spaceship.IsEngineOn = true;
            }

            int cooldownEndTick = tick;
            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Frozen, out int talentIndex))
            {
                LogService.LogError($"No Frozen talent found for player id {_casterPlayerId}");
            }
            else
            {
                ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
                cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
                talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
            }

            _netEventsDataService.AddDeactivateFrozenTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
            _playersInLavaTrackerService.TryResetPlayerTimePassedSinceLastDamageTaken(_casterPlayerId);

            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();

            // A spike the player entered while immune never damaged them, so collide with it now that immunity ended.
            _tryCollidePlayerWithOverlappingSpikeCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
