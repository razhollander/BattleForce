using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Features.PowerUps.Scripts.Views
{
    public class PowerUpBallView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _sprites;

        public ushort Id { get; private set; }

        public void Init(ushort id, PowerUpType type)
        {
            Id = id;
            if (_sprites != null && (int)type >= 0 && (int)type < _sprites.Length)
            {
                _spriteRenderer.sprite = _sprites[(int)type];
            }
        }

        public void UpdatePosition(Vector2 position)
        {
            transform.position = new UnityEngine.Vector3(position.X, position.Y, 0);
        }
    }
}
