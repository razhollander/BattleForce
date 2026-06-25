using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class SonicSlapPowerUpController : IPowerUpController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly FixedUnorderedList<ushort> _cachedAffectedPlayerIds;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.SonicSlap;

        public SonicSlapPowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _cachedAffectedPlayerIds = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentEnemyPlayers);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void OnTick(int tick) { }

        public void Perform(int tick)
        {
            _matchDataService.SimulationState.SetIsPowerUpCurrentlyActiveForPlayer(_casterPlayerId, true);
            var casterTeamId = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).TeamId;
            _cachedAffectedPlayerIds.Clear();

            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isEnemyPlayer = playerState.TeamId != casterTeamId;
                if (!isEnemyPlayer)
                {
                    continue;
                }

                playerState.Spaceship.Transform.Direction = -playerState.Spaceship.Transform.Direction;
                playerState.Spaceship.Transform.Velocity = -playerState.Spaceship.Transform.Velocity;

                ref var affectedPlayerId = ref _cachedAffectedPlayerIds.AddAndGet();
                affectedPlayerId = playerState.Id;
            }

            _netEventsDataService.AddSonicSlapActivatedNetEvent(tick, _casterPlayerId, _cachedAffectedPlayerIds);
            _matchDataService.SimulationState.SetIsPowerUpCurrentlyActiveForPlayer(_casterPlayerId, false);
        }
    }
}
