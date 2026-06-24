using System.Threading;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class SonicSnapEffectView : MonoBehaviour
    {
        private const string ACTIVATE_ANIMATION = "ActivateSonicSnap";

        [SerializeField] private Animation _animation;

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public async Awaitable PlaySnapEffect(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);

            try
            {
                await _animation.PlayAsync(ACTIVATE_ANIMATION, cancellationToken);
            }
            finally
            {
                Hide();
            }
        }
    }
}
