using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public class PlayerModel
    {
        public int Id;
        public int CurrentHealth;
        public int MaxHealth;
        public Vector2 Position;
        public Quaternion Rotation;
    }
}