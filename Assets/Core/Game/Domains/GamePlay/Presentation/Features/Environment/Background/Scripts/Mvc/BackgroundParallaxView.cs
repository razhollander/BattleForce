using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc
{
    public class BackgroundParallaxView : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundLayer1Transform;
        [SerializeField] private Transform _backgroundLayer2Transform;
        [SerializeField] private float _layer1OffsetMultiplier = 1;
        [SerializeField] private float _layer2OffsetMultiplier = 2;
        [SerializeField] private float _OffsetMultiplier = 1;

        public void MoveLayers(Vector2 offset)
        {
            _backgroundLayer1Transform.SetPositionXY(offset * (_layer1OffsetMultiplier * _OffsetMultiplier));
            _backgroundLayer2Transform.SetPositionXY(offset * (_layer2OffsetMultiplier * _OffsetMultiplier));
        }
    }
}