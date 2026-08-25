using System.Numerics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public class PlayersMoveDestinationPointService : IPlayersMoveDestinationPointService
    {
        private readonly CapacityDict<ushort, PlayerMoveDestinationPointData> _destinationPointPerPlayer;

        public PlayersMoveDestinationPointService(NetworkConfig networkConfig)
        {
            _destinationPointPerPlayer = new CapacityDict<ushort, PlayerMoveDestinationPointData>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void SetPlayerMoveDestinationPoint(ushort playerId, Vector2 destinationPoint, Vector2 playerDirection)
        {
            _destinationPointPerPlayer[playerId] = new PlayerMoveDestinationPointData
            {
                DestinationPoint = destinationPoint,
                DirectionAfterLastRotation = playerDirection,
            };
        }

        public void ClearPlayerMoveDestinationPoint(ushort playerId)
        {
            _destinationPointPerPlayer.Remove(playerId);
        }

        public bool TryGetPlayerMoveDestinationPoint(ushort playerId, out PlayerMoveDestinationPointData destinationPointData)
        {
            return _destinationPointPerPlayer.TryGetValue(playerId, out destinationPointData);
        }

        public void SetPlayerRotatedDirection(ushort playerId, Vector2 rotatedDirection)
        {
            if (!_destinationPointPerPlayer.TryGetValue(playerId, out var destinationPointData))
            {
                return;
            }

            destinationPointData.DirectionAfterLastRotation = rotatedDirection;
            _destinationPointPerPlayer[playerId] = destinationPointData;
        }
    }
}
