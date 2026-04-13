using UnityEngine;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class PlayerAssistLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int _pointsCount = 30;
        [SerializeField] private float _lerpSpeed = 15f; // Adjust for smoothness

        private Vector3[] _targetPositions;

        public void OnCreated()
        {
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = _pointsCount;
            _targetPositions = new Vector3[_pointsCount];
        }

        public void UpdateLine(Vector2 headPos, Vector2 direction, float angularVelocity, float velocityLength, float lineLength)
        {
            if (_pointsCount <= 1 || Mathf.Approximately(lineLength, 0f))
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            if (_lineRenderer.positionCount != _pointsCount)
                _lineRenderer.positionCount = _pointsCount;

            float distanceStep = lineLength / (_pointsCount - 1);
            float speed = Mathf.Max(velocityLength, 5f);
            float timeStep = distanceStep / speed;

            Vector2 currentPos = headPos;
            Vector2 currentDir = direction;

            // 1. Calculate and store the "Target" world positions
            for (int i = 0; i < _pointsCount; i++)
            {
                _targetPositions[i] = new Vector3(currentPos.x, currentPos.y, 0f);

                currentPos += currentDir * distanceStep;

                if (Mathf.Abs(angularVelocity) > 0.01f)
                {
                    float rotationAngle = angularVelocity * timeStep;
                    currentDir = currentDir.Rotate(rotationAngle);
                }
            }
        }

        private void Update()
        {
            // 2. Every frame, Lerp the LineRenderer's current positions toward the targets
            if (_lineRenderer.positionCount == 0 || _targetPositions == null) return;

            for (int i = 0; i < _pointsCount; i++)
            {
                Vector3 currentPointPos = _lineRenderer.GetPosition(i);
                
                // Smoothly interpolate between current and target
                Vector3 lerpedPos = Vector3.Lerp(currentPointPos, _targetPositions[i], Time.deltaTime * _lerpSpeed);
                
                _lineRenderer.SetPosition(i, lerpedPos);
            }
        }

        public void SetColor(Color color)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = color;
                _lineRenderer.endColor = color;
            }
        }

        public void SetIsShown(bool isShown)
        {
            gameObject.SetActive(isShown);
        }
    }
}