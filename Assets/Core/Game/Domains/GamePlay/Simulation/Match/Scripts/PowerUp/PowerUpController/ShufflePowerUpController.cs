using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class ShufflePowerUpController : IPowerUpController
    {
        private const int SwapIntervalInTicks = 15;

        private struct PendingPlayerSwap
        {
            public ushort PlayerId1;
            public ushort PlayerId2;
        }

        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly FixedUnorderedList<ushort> _cachedPlayerIds;
        private readonly PendingPlayerSwap[] _pendingSwaps;
        private int _pendingSwapsCount;
        private int _nextSwapIndex;
        private int _nextSwapTick;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.Shuffle;

        private bool IsSequenceInProgress => _nextSwapIndex < _pendingSwapsCount;

        public ShufflePowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _cachedPlayerIds = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentPlayers);
            _pendingSwaps = new PendingPlayerSwap[networkConfig.MaxCap.ConcurrentPlayers];
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void Perform(int tick)
        {
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
                _nextSwapTick = tick + SwapIntervalInTicks;
            }
            else
            {
                _matchDataService.SimulationState.SetIsPowerUpCurrentlyActiveForPlayer(_casterPlayerId, false);
                _netEventsDataService.AddActivateShufflePowerUpNetEvent(tick, _casterPlayerId);
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
            _pendingSwapsCount = 0;
            var playerCount = _cachedPlayerIds.Count;
            for (int i = playerCount - 1; i > 0; i--)
            {
                var randomIndex = (int)RNG.NextFloat(0f, i + 1f);
                if (randomIndex == i)
                    continue;

                _pendingSwaps[_pendingSwapsCount++] = new PendingPlayerSwap
                {
                    PlayerId1 = _cachedPlayerIds.Get(i),
                    PlayerId2 = _cachedPlayerIds.Get(randomIndex)
                };
            }
        }

        private void ExecuteNextSwap(int tick)
        {
            var swap = _pendingSwaps[_nextSwapIndex++];

            var state1 = _matchDataService.SimulationState.GetPlayerById(swap.PlayerId1);
            var state2 = _matchDataService.SimulationState.GetPlayerById(swap.PlayerId2);

            var tempPosition = state1.Spaceship.Transform.Position;
            state1.Spaceship.Transform.Position = state2.Spaceship.Transform.Position;
            state2.Spaceship.Transform.Position = tempPosition;

            _netEventsDataService.AddShuffleSwapPlayerPositionNetEvent(tick, _casterPlayerId);
        }
    }
}
