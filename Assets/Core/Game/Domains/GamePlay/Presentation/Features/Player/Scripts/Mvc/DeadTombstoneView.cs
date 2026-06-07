using System.Threading;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class DeadTombstoneView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _stone;
        [SerializeField] private float _stoneStartYPosition = -1000f;
        [SerializeField] private float _stoneFallDurationInSeconds = 0.5f;
        [SerializeField] private Ease _stoneFallEase = Ease.InOutSine;
    
        [Button("Play")]
        public void PlayShowAnimation()
        {
            PlayShowAnimation(new CancellationToken()).Forget();
        }
    
        public async Awaitable PlayShowAnimation(CancellationToken cancellationToken)
        {
            _stone.transform.localPosition = new Vector3(_stone.transform.localPosition.x, _stoneStartYPosition);
            await _stone.transform.DOLocalMove(Vector3.zero, _stoneFallDurationInSeconds).SetEase(_stoneFallEase).WithCancellationSafe(cancellationToken);
        }

        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}
