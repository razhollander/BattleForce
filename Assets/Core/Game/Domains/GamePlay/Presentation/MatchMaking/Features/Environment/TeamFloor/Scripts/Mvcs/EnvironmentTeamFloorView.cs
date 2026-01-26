using System;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using DG.Tweening;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class EnvironmentTeamFloorView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _bounceaAnimationScale = 1.2f;
        [SerializeField] private float _bounceaAnimationDuration = 0.15f;
        [SerializeField] private int _bounceaAnimationLoops = 2;
        
        private CancellationTokenSource _bounceAnimationCancellationTokenSource;
        
        public void Setup(Mesh mesh, Color color)
        {
            _meshFilter.sharedMesh = mesh;
            _meshRenderer.material.color = color;
        }

        public void AnimateBounce()
        {
            _ = AnimateBounceAsync();
        }

        private async Awaitable AnimateBounceAsync()
        {
            _bounceAnimationCancellationTokenSource?.Cancel();
            _bounceAnimationCancellationTokenSource = new CancellationTokenSource();

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
