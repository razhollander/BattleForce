using System.Threading;
using CoreDomain.Scripts.Utils;
using DG.Tweening;
using UnityEngine;

namespace CoreDomain.Scripts.Extensions
{
    public static class DOTweenExtensions
    {
        public static async Awaitable WithCancellationSafe(this Tween tween, CancellationToken cancellationToken)
        {
            KillTweenImmediatelyWhenTokenIsCanceled(tween, cancellationToken); // the tween is killed 1 frame after the token is canceled, so this prevents it
            await WaitUntilCompleted(tween, cancellationToken); 
            cancellationToken.ThrowIfCancellationRequested(); // when the cancellationToken is cancelled the tween stops, BUT there is no throw so we throw afterwards
        }

        private static async Awaitable WaitUntilCompleted(this Tween tween, CancellationToken cancellationToken)
        {
            await AwaitableUtils.WaitUntil(() => !tween.active || tween.IsComplete(), cancellationToken); 
        }

        private static void KillTweenImmediatelyWhenTokenIsCanceled(this Tween tween, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            });
        }
    }
}
