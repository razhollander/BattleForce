using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
    }
}
