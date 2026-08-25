using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface IPlayersMoveDestinationPointService
    {
        void SetPlayerMoveDestinationPoint(ushort playerId, Vector2 destinationPoint, Vector2 playerDirection);
        void ClearPlayerMoveDestinationPoint(ushort playerId);
        bool TryGetPlayerMoveDestinationPoint(ushort playerId, out PlayerMoveDestinationPointData destinationPointData);
        void SetPlayerRotatedDirection(ushort playerId, Vector2 rotatedDirection);
    }

    public struct PlayerMoveDestinationPointData
    {
        public Vector2 DestinationPoint;
        public Vector2 DirectionAfterLastRotation;
    }
}
