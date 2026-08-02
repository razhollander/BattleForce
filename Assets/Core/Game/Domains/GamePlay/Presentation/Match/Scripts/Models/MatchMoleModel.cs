using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchMoleModel
    {
        public ushort Id;
        public Vector2 Position;

        public MatchMoleModel(ushort id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }
}
