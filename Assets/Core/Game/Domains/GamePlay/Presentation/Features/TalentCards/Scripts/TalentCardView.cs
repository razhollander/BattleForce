using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _fullHealthSprite;
        [SerializeField] private Sprite _damagedSprite;

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public void SwapToDamagedSprite()
        {
            _spriteRenderer.sprite = _damagedSprite;
        }

        public void SwapToFullHealthSprite()
        {
            _spriteRenderer.sprite = _fullHealthSprite;
        }
    }
}
