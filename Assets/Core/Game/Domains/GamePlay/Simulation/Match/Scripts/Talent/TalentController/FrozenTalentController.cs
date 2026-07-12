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
    // Frozen turns the caster into an inert, invulnerable block that keeps whatever velocity/spin it had:
    // it takes no damage, ignores lava, doesn't decelerate, has its engine off, ignores all inputs (except a
    // second talent press) and can only change its facing through the physical result of its angular velocity.
    // Most of those rules live in the systems that own them (see PlayerHitCommand, PlayersDecelerationLogic,
    // PlayersEngineLogic, TrySpinPlayerCommand, ProcessCachedCollisionsCommand, MatchPlayerInputsPacketsHandler,
    // StepTimersCommand); they all key off GetIsTalentCurrentlyActiveForPlayer(playerId, TalentType.Frozen).
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

            // A second press cancels the talent early (this is the only input honored while frozen).
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

            // The engine is turned off but motion is intentionally NOT stopped: a frozen ship keeps sliding/spinning.
            casterPlayerState.Spaceship.IsEngineOn = false;

            _netEventsDataService.AddActivateFrozenTalentNetEvent(tick, _casterPlayerId);

            // Frozen grants lava immunity: if the player activated it while standing in lava, stop the exposed state.
            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            // Keep the engine off for the whole duration; the deceleration/engine systems leave a frozen player alone.
            _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).Spaceship.IsEngineOn = false;

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

            // Immunity ends with Frozen: if the player is still in lava, resume the exposed state.
            _updatePlayerLavaExposureCommand.SetPlayerId(_casterPlayerId).SetProcessedTick(tick).Execute();
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
