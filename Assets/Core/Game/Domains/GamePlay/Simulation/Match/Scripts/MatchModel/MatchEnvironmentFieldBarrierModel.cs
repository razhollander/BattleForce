using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentFieldBarrierModel
    {
        public Vector2 Position;
        public Vector2 Size;
        public FieldBarrierShape Shape;
        public ushort TeamId;
        public ushort Id;

        public MatchEnvironmentFieldBarrierModel()
        {
        }

        public MatchEnvironmentFieldBarrierModel(ushort id, ushort teamId, Vector2 position, Vector2 size, FieldBarrierShape shape)
        {
            Id = id;
            TeamId = teamId;
            Position = position;
            Size = size;
            Shape = shape;
        }
    }
}
