using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class EnvironmentTeamFloorsView : MonoBehaviour
    {
        [SerializeField] private float _bounceaAnimationScale = 1.2f;
        [SerializeField] private float _bounceaAnimationDuration = 0.15f;
        [SerializeField] private int _bounceaAnimationLoops = 2;
        [SerializeField] private EnvironmentTeamFloorView _environmentTeamFloorViewPrefab;
        
        private CancellationTokenSource _bounceAnimationCancellationTokenSource;

        public void CreateFloor(Mesh mesh, ushort wallId, Color color)
        {
            var teamFloorView = UnityEngine.Object.Instantiate(_environmentTeamFloorViewPrefab, transform);
            teamFloorView.name = "EnvironmentTeamFloor_" + wallId;
            teamFloorView.Setup(mesh, color);
        }

        public void AnimateBounce()
        {
            _ = AnimateBounceAsync();
        }

        private async Awaitable AnimateBounceAsync()
        {
            _bounceAnimationCancellationTokenSource?.Cancel();
            _bounceAnimationCancellationTokenSource = new CancellationTokenSource();
            transform.localScale = Vector3.one;
            
            try
            {
                await transform
                    .DOScale(_bounceaAnimationScale, _bounceaAnimationDuration)
                    .SetLoops(_bounceaAnimationLoops, LoopType.Yoyo)
                    .WithCancellationSafe(_bounceAnimationCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                LogService.LogException(e);
            }
        }
    }
}