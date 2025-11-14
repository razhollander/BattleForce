using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace CoreDomain.Scripts.Extensions
{
    public static class AnimationExtensions
    {
        public static async Awaitable PlayAsync(this Animation animation, string clipName, CancellationToken cancellationToken = default, AwaitableCompletionSource completionSource = null)
        {
            if (animation == null)
            {
                LogService.LogError($"animation for clip {clipName} is null");
                return;
            }

            var clip = animation.GetClip(clipName);
            if (clip == null)
            {
                LogService.LogError($"clip {clipName} is null");
                return;
            }
            
            animation.Play(clipName);
            var waitForClipDuration = Awaitable.WaitForSecondsAsync(clip.length, cancellationToken: cancellationToken);
            var waitForClipChanged = AwaitableUtils.WaitUntil(() => GetAnimationStateName(animation) != clipName, cancellationToken);
            await new []{waitForClipDuration, waitForClipChanged}.WhenAny(cancellationToken);
            completionSource?.TrySetResult();
        }
        
        private static string GetAnimationStateName(Animation animation)
        {
            if (animation == null)
            {
                return default;
            }
            
            foreach (AnimationState state in animation)
            {
                if (animation.IsPlaying(state.name))
                {
                    return state.name;
                }
            }
            
            return default;
        }
     
    }
}
