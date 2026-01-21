using System;
using CoreDomain.Scripts.Helpers.Pools;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.TalentCards.Scripts
{
    public class TalentCardView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _talentSpriteRenderer;
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;
        [SerializeField] private Sprite _fullHealthSprite;
        [SerializeField] private Sprite _damagedSprite;

        public void SetTalentSprite(Sprite sprite)
        {
            _talentSpriteRenderer.sprite = sprite;
        }

        public void SwapToDamagedSprite()
        {
            _backgroundSpriteRenderer.sprite = _damagedSprite;
        }

        public void SwapToFullHealthSprite()
        {
            _backgroundSpriteRenderer.sprite = _fullHealthSprite;
        }

        public void OnCreated()
        {
        }

        public Action Despawn { get; set; }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
