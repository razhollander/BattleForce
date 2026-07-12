using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    // Single source of truth for a player's lava-exposure state (the burning visual + damage flag).
    // A player is exposed while physically in lava AND not in Rock state. Rock makes the player immune,
    // so it must not show as exposed. This reconciles that rule regardless of what changed (entering/leaving
    // lava, or Rock toggling on/off) and only emits a net event when the state actually flips.
    public class UpdatePlayerLavaExposureCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;

        private ushort _playerId;
        private int _processedTick;

        public UpdatePlayerLavaExposureCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public UpdatePlayerLavaExposureCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
        }

        public void Execute()
        {
            var spaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            var isPlayerRockActive = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_playerId, TalentType.Rock);
            var isPlayerFrozenActive = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_playerId, TalentType.Frozen);
            var isPlayerLavaImmune = isPlayerRockActive || isPlayerFrozenActive;
            var shouldBeExposed = _playersInLavaTrackerService.IsPlayerInLava(_playerId) && !isPlayerLavaImmune;

            var didPlayerExposedStateChange = shouldBeExposed != spaceship.IsExposedToLava;

            if (!didPlayerExposedStateChange)
            {
                return;
            }

            spaceship.IsExposedToLava = shouldBeExposed;
            if (shouldBeExposed)
            {
                _netEventsDataService.AddPlayerStartedExposedToLavaNetEvent(_processedTick, _playerId);
            }
            else
            {
                _netEventsDataService.AddPlayerEndedExposedToLavaNetEvent(_processedTick, _playerId);
            }
        }
    }
}
