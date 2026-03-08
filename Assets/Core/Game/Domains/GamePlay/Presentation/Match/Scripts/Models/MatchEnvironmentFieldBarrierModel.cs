using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentFieldBarrierModel
    {
        public ushort Id { get; set; }
        public ushort TeamId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public float CircleRadius => Size.X;
        public FieldBarrierShape Shape { get; set; }

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
