using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class ShufflePowerUpController : IPowerUpController
    {
        private struct PendingPlayerSwap
        {
            public ushort PlayerId1;
            public ushort PlayerId2;
        }

        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly NetworkConfig _networkConfig;
        private readonly FixedUnorderedList<ushort> _cachedPlayerIds;
        private readonly PendingPlayerSwap[] _pendingSwaps;
        private readonly ushort[] _shuffleBuffer;
        private int _pendingSwapsCount;
        private int _nextSwapIndex;
        private int _nextSwapTick;
        private ushort _casterPlayerId;
        private readonly ISimulationGamePlayConfigService _simulationGamePlayConfigService;

        public PowerUpType PowerUpType => PowerUpType.Shuffle;

        private bool IsSequenceInProgress => _nextSwapIndex < _pendingSwapsCount;

        public ShufflePowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService, NetworkConfig networkConfig,
            ISimulationGamePlayConfigService simulationGamePlayConfigService)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _networkConfig = networkConfig;
            _simulationGamePlayConfigService = simulationGamePlayConfigService;
            _cachedPlayerIds = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentPlayers);
            _pendingSwaps = new PendingPlayerSwap[networkConfig.MaxCap.ConcurrentPlayers];
            _shuffleBuffer = new ushort[networkConfig.MaxCap.ConcurrentPlayers];
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void Perform(int tick)
        {
            if (IsSequenceInProgress)
            {
                return;
            }
            
            CollectPlayerIds();
            PreCalculateSwapPairs();
            _nextSwapIndex = 0;
            _nextSwapTick = tick;
            _matchDataService.SimulationState.SetIsPowerUpCurrentlyActiveForPlayer(_casterPlayerId, true);
        }

        public void OnTick(int tick)
        {
            if (!IsSequenceInProgress || tick < _nextSwapTick)
                return;

            ExecuteNextSwap(tick);

            if (IsSequenceInProgress)
            {
                _nextSwapTick = TickUtils.GetTickPassedAfterDuration(tick, _simulationGamePlayConfigService.GamePlayConfig.PowerUps.ShuffleSwapIntervalInSeconds, _networkConfig.DeltaTime);
            }
            else
            {
                _matchDataService.SimulationState.SetIsPowerUpCurrentlyActiveForPlayer(_casterPlayerId, false);
                _netEventsDataService.AddDectivateShufflePowerUpNetEvent(tick, _casterPlayerId);
            }
        }

        private void CollectPlayerIds()
        {
            _cachedPlayerIds.Clear();
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                ref var playerId = ref _cachedPlayerIds.AddAndGet();
                playerId = playerState.Id;
            }
        }

        private void PreCalculateSwapPairs()
        {
            var playerCount = _cachedPlayerIds.Count;
            for (int i = 0; i < playerCount; i++)
                _shuffleBuffer[i] = _cachedPlayerIds.GetByIndex(i);

            _pendingSwapsCount = 0;

            // Sattolo's algorithm: NextInt(0, i) (exclusive of i) forces a single N-cycle,
            // guaranteeing every player ends up at a different position.
            for (int i = playerCount - 1; i > 0; i--)
            {
                var randomIndex = RNG.NextInt(0, i);

                _pendingSwaps[_pendingSwapsCount++] = new PendingPlayerSwap
                {
                    PlayerId1 = _shuffleBuffer[i],
                    PlayerId2 = _shuffleBuffer[randomIndex]
                };

                (_shuffleBuffer[i], _shuffleBuffer[randomIndex]) = (_shuffleBuffer[randomIndex], _shuffleBuffer[i]);
            }
        }

        private void ExecuteNextSwap(int tick)
        {
            var swap = _pendingSwaps[_nextSwapIndex++];

            var spaceship1 = _matchDataService.SimulationState.GetPlayerById(swap.PlayerId1).Spaceship;
            var spaceship2 = _matchDataService.SimulationState.GetPlayerById(swap.PlayerId2).Spaceship;

            (spaceship1.Transform.Position, spaceship2.Transform.Position) = (spaceship2.Transform.Position, spaceship1.Transform.Position);

            _netEventsDataService.AddShuffleSwapPlayerPositionNetEvent(tick, _casterPlayerId);
        }
    }
}
