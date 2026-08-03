using System;
using CoreDomain.Scripts.Helpers.Pools;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    /// <summary>
    /// The state is carried by the sprite, the legacy clip only adds the pop and squash motion.
    /// Legacy animation cannot drive sprite curves, so the sprite is assigned here instead.
    /// </summary>
    public class MoleView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _inHoleSprite;
        [SerializeField] private Sprite _outsideHoleSprite;
        [SerializeField] private Sprite _hitSprite;

        [Header("Motion")]
        [SerializeField] private Animation _animation;
        [SerializeField] private string _inHoleAnimationClipName = "MoleInHole";
        [SerializeField] private string _outsideHoleAnimationClipName = "MoleOutsideHole";
        [SerializeField] private string _hitAnimationClipName = "MoleHit";

        public Action Despawn { get; set; }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void PlayState(MoleStateType stateType)
        {
            _spriteRenderer.sprite = GetSprite(stateType);
            _animation.Play(GetAnimationClipName(stateType));
        }

        private Sprite GetSprite(MoleStateType stateType)
        {
            switch (stateType)
            {
                case MoleStateType.InHole:
                    return _inHoleSprite;
                case MoleStateType.OutsideHole:
                    return _outsideHoleSprite;
                case MoleStateType.Hit:
                    return _hitSprite;
                default:
                    LogService.LogError("Not implemented mole state type: " + stateType);
                    return _inHoleSprite;
            }
        }

        private string GetAnimationClipName(MoleStateType stateType)
        {
            switch (stateType)
            {
                case MoleStateType.InHole:
                    return _inHoleAnimationClipName;
                case MoleStateType.OutsideHole:
                    return _outsideHoleAnimationClipName;
                case MoleStateType.Hit:
                    return _hitAnimationClipName;
                default:
                    LogService.LogError("Not implemented mole state type: " + stateType);
                    return _inHoleAnimationClipName;
            }
        }

        public void OnCreated()
        {
        }

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
